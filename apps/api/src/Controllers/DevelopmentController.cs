using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using TaskHub.Api.Core.Interfaces.Repositories;
using TaskHub.Api.Core.Interfaces;
using TaskHub.Api.Models.Entities.Users;
using TaskHub.Api.Models.Entities.Enums;

using TaskHub.Api.Common;

namespace TaskHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [DevelopmentOnly] // This controller is only available in the Development environment
    public class DevelopmentController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DevelopmentController(IConfiguration configuration, IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _configuration = configuration;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Creates a test user for development purposes. FOR DEVELOPMENT USE ONLY.
        /// </summary>
        /// <returns>The created user information.</returns>
        [HttpPost("create-test-user")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public async Task<IActionResult> CreateTestUser()
        {
            // Check if test user already exists
            var existingUser = await _userRepository.GetByEmailAsync("test@example.com");
            if (existingUser != null)
            {
                return Ok(new { 
                    id = existingUser.Id, 
                    email = existingUser.Email,
                    message = "Test user already exists"
                });
            }

            // Create test user
            var testUser = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                FirstName = "Test",
                LastName = "User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(testUser);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { 
                id = testUser.Id, 
                email = testUser.Email,
                message = "Test user created successfully"
            });
        }

        /// <summary>
        /// Generates a JWT for a test user or creates one if needed. FOR DEVELOPMENT USE ONLY.
        /// </summary>
        /// <param name="userId">Optional user ID to include in the token. If not provided, uses/creates a test user.</param>
        /// <param name="isAdmin">If true, the token will contain an 'admin' role claim.</param>
        /// <returns>A JWT string.</returns>
        [HttpGet("generate-token")]
        [ApiExplorerSettings(IgnoreApi = false)] // Ensure this is visible in Swagger
        public async Task<IActionResult> GenerateToken([FromQuery] Guid? userId, [FromQuery] bool isAdmin = false)
        {
            Guid actualUserId;

            if (userId.HasValue)
            {
                // Use the provided user ID
                actualUserId = userId.Value;
            }
            else
            {
                // Get or create test user
                var testUser = await _userRepository.GetByEmailAsync("test@example.com");
                if (testUser == null)
                {
                    testUser = new User
                    {
                        Username = "testuser",
                        Email = "test@example.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                        FirstName = "Test",
                        LastName = "User",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                                         await _userRepository.AddAsync(testUser);
                     await _unitOfWork.SaveChangesAsync();
                }
                actualUserId = testUser.Id;
            }

            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key not configured")));

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, actualUserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, actualUserId.ToString()) // Set the user's ID in the Name claim
            };

            if (isAdmin)
            {
                claims.Add(new Claim(ClaimTypes.Role, "admin"));
            }

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(Convert.ToDouble(jwtSettings["ExpireDays"])),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            return Ok(new { 
                token = tokenHandler.WriteToken(token),
                userId = actualUserId,
                email = (await _userRepository.GetByIdAsync(actualUserId))?.Email
            });
        }

        /// <summary>
        /// Validates JWT token and returns user information. FOR DEVELOPMENT USE ONLY.
        /// </summary>
        /// <returns>User information from the JWT token.</returns>
        [HttpGet("validate-token")]
        [Authorize]
        [ApiExplorerSettings(IgnoreApi = false)]
        public IActionResult ValidateToken()
        {
            var userId = User?.Identity?.Name;
            var isAuthenticated = User?.Identity?.IsAuthenticated ?? false;
            var claims = User?.Claims?.Select(c => new { c.Type, c.Value }).ToArray() ?? Array.Empty<object>();

            return Ok(new
            {
                IsAuthenticated = isAuthenticated,
                UserId = userId,
                Claims = claims,
                Identity = new
                {
                    Name = User?.Identity?.Name,
                    AuthenticationType = User?.Identity?.AuthenticationType,
                    IsAuthenticated = User?.Identity?.IsAuthenticated
                }
            });
        }

        /// <summary>
        /// Lists all users in the database. FOR DEVELOPMENT USE ONLY.
        /// </summary>
        /// <returns>List of all users with basic information.</returns>
        [HttpGet("list-users")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public async Task<IActionResult> ListUsers()
        {
            var users = await _userRepository.GetAllAsync();
            var userList = users.Select(u => new 
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Role = u.Role.ToString(),
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                HasPassword = !string.IsNullOrEmpty(u.PasswordHash)
            }).ToList();

            return Ok(new 
            {
                Count = userList.Count,
                Users = userList
            });
        }

        /// <summary>
        /// Manually triggers database seeding. FOR DEVELOPMENT USE ONLY.
        /// </summary>
        /// <returns>Result of the seeding operation.</returns>
        [HttpPost("seed-database")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public async Task<IActionResult> SeedDatabase()
        {
            try
            {
                var existingUsers = await _userRepository.GetAllAsync();
                if (existingUsers.Any())
                {
                    return Ok(new { message = $"Database already contains {existingUsers.Count()} users. Seeding skipped." });
                }

                // Manually create the seed data
                var adminUser = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "admin",
                    Email = "admin@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin"),
                    Role = UserRole.Admin,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var demoUser = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "demo",
                    Email = "demo@taskhub.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Demo123!"),
                    Role = UserRole.User,
                    FirstName = "Demo",
                    LastName = "User",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _userRepository.AddAsync(adminUser);
                await _userRepository.AddAsync(demoUser);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { 
                    message = "Database seeded successfully",
                    usersCreated = new[] { 
                        new { adminUser.Username, adminUser.Email },
                        new { demoUser.Username, demoUser.Email }
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message, details = ex.ToString() });
            }
        }

        /// <summary>
        /// Tests password verification with current database users. FOR DEVELOPMENT USE ONLY.
        /// </summary>
        /// <returns>Password verification test results.</returns>
        [HttpPost("test-passwords")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public async Task<IActionResult> TestPasswords()
        {
            try
            {
                var users = await _userRepository.GetAllAsync();
                var results = new List<object>();

                foreach (var user in users)
                {
                    // Test various password combinations
                    var testPasswords = new[] { "admin", "Demo123!", "password123" };
                    var passwordResults = new List<object>();

                    foreach (var testPassword in testPasswords)
                    {
                        var isValid = !string.IsNullOrEmpty(user.PasswordHash) && 
                                     BCrypt.Net.BCrypt.Verify(testPassword, user.PasswordHash);
                        passwordResults.Add(new { password = testPassword, isValid });
                    }

                    results.Add(new 
                    {
                        user.Username,
                        user.Email,
                        hasPasswordHash = !string.IsNullOrEmpty(user.PasswordHash),
                        passwordHashLength = user.PasswordHash?.Length ?? 0,
                        passwordHashPrefix = user.PasswordHash?.Substring(0, Math.Min(10, user.PasswordHash?.Length ?? 0)),
                        testResults = passwordResults
                    });
                }

                return Ok(new { users = results });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message, details = ex.ToString() });
            }
        }

        /// <summary>
        /// Clears all users and reseeds with fresh verified credentials. FOR DEVELOPMENT USE ONLY.
        /// </summary>
        /// <returns>Result of the reset operation.</returns>
        [HttpPost("reset-users")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public async Task<IActionResult> ResetUsers()
        {
            try
            {
                // Clear all existing users
                var existingUsers = await _userRepository.GetAllAsync();
                foreach (var user in existingUsers)
                {
                    _userRepository.Delete(user);
                }
                await _unitOfWork.SaveChangesAsync();

                // Create admin user with verified password hash
                var adminPassword = "admin";
                var adminHash = BCrypt.Net.BCrypt.HashPassword(adminPassword);
                var adminUser = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "admin",
                    Email = "admin@example.com",
                    PasswordHash = adminHash,
                    Role = UserRole.Admin,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Create demo user with verified password hash  
                var demoPassword = "Demo123!";
                var demoHash = BCrypt.Net.BCrypt.HashPassword(demoPassword);
                var demoUser = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "demo",
                    Email = "demo@taskhub.com",
                    PasswordHash = demoHash,
                    Role = UserRole.User,
                    FirstName = "Demo",
                    LastName = "User",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Verify password hashes before saving
                var adminVerification = BCrypt.Net.BCrypt.Verify(adminPassword, adminHash);
                var demoVerification = BCrypt.Net.BCrypt.Verify(demoPassword, demoHash);

                if (!adminVerification || !demoVerification)
                {
                    return BadRequest(new { 
                        error = "Password verification failed during creation",
                        adminVerification,
                        demoVerification 
                    });
                }

                await _userRepository.AddAsync(adminUser);
                await _userRepository.AddAsync(demoUser);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { 
                    message = "Users reset successfully with verified credentials",
                    users = new[] { 
                        new { 
                            adminUser.Username, 
                            adminUser.Email, 
                            password = adminPassword,
                            hashVerified = adminVerification
                        },
                        new { 
                            demoUser.Username, 
                            demoUser.Email, 
                            password = demoPassword,
                            hashVerified = demoVerification
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message, details = ex.ToString() });
            }
        }

        /// <summary>
        /// Ensures the database is seeded with initial data. FOR DEVELOPMENT USE ONLY.
        /// </summary>
        /// <returns>Result of the seeding operation.</returns>
        [HttpPost("ensure-seed-data")]
        [Authorize(Roles = "Admin")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public async Task<IActionResult> EnsureSeedData()
        {
            try
            {
                var usersExist = await _userRepository.GetAllAsync();
                if (usersExist.Any())
                {
                    return Ok(new { message = "Database already contains users. Seeding skipped." });
                }

                // Manually create the seed data
                var adminUser = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "admin",
                    Email = "admin@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin"),
                    Role = UserRole.Admin,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var demoUser = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "demo",
                    Email = "demo@taskhub.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Demo123!"),
                    Role = UserRole.User,
                    FirstName = "Demo",
                    LastName = "User",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _userRepository.AddAsync(adminUser);
                await _userRepository.AddAsync(demoUser);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { 
                    message = "Database seeded successfully",
                    usersCreated = new[] { 
                        new { adminUser.Username, adminUser.Email },
                        new { demoUser.Username, demoUser.Email }
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message, details = ex.ToString() });
            }
        }
    }
}
