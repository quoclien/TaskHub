using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using TaskHub.Api.Core.Interfaces.Repositories;
using TaskHub.Api.Core.Interfaces;
using TaskHub.Api.Models.Entities.Users;

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
    }
}
