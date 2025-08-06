using Microsoft.AspNetCore.Mvc;
using TaskHub.Api.Core.Interfaces;
using TaskHub.Api.Core.Interfaces.Repositories;
using TaskHub.Api.Models.Dtos.Users;
using TaskHub.Api.Models.Entities.Users;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;

namespace TaskHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NextAuthController : ApiControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;
        private readonly IMapper _mapper;
        private readonly ILogger<NextAuthController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public NextAuthController(
            IUserRepository userRepository,
            IAuthService authService,
            IMapper mapper,
            ILogger<NextAuthController> logger,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        /// <summary>
        /// Handle Google OAuth user from NextAuth.js
        /// Creates or updates user based on Google OAuth data
        /// </summary>
        /// <param name="request">Google OAuth user data from NextAuth.js</param>
        /// <returns>User data for NextAuth.js session</returns>
        [HttpPost("google-oauth")]
        public async Task<IActionResult> HandleGoogleOAuth([FromBody] GoogleOAuthRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email))
                    return BadRequest("Email is required");

                // Check if user exists
                var existingUser = await _userRepository.GetByEmailAsync(request.Email);
                
                User user;
                if (existingUser != null)
                {
                    // Update existing user with Google data
                    existingUser.FirstName = request.Name?.Split(' ').FirstOrDefault() ?? existingUser.FirstName;
                    existingUser.LastName = request.Name?.Split(' ').Skip(1).FirstOrDefault() ?? existingUser.LastName;
                    existingUser.AvatarUrl = request.Image ?? existingUser.AvatarUrl;
                    existingUser.LastLoginAt = DateTime.UtcNow;
                    existingUser.UpdatedAt = DateTime.UtcNow;
                    existingUser.GoogleId = request.Sub;

                    _userRepository.Update(existingUser);
                    user = existingUser;
                }
                else
                {
                    // Create new user from Google OAuth data
                    var nameParts = request.Name?.Split(' ') ?? new[] { request.Email };
                    user = new User
                    {
                        Email = request.Email,
                        Username = request.Email, // Use email as username for OAuth users
                        FirstName = nameParts.FirstOrDefault() ?? "User",
                        LastName = nameParts.Skip(1).FirstOrDefault() ?? "",
                        AvatarUrl = request.Image,
                        GoogleId = request.Sub,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        LastLoginAt = DateTime.UtcNow,
                        // No password hash needed for OAuth users
                        PasswordHash = null
                    };

                    await _userRepository.AddAsync(user);
                }

                await _unitOfWork.SaveChangesAsync();

                var userDto = _mapper.Map<UserDto>(user);
                _logger.LogInformation("Google OAuth user {Email} processed successfully", request.Email);

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Google OAuth for {Email}", request.Email);
                return StatusCode(500, "An error occurred processing the OAuth request");
            }
        }

        /// <summary>
        /// Validate user session for NextAuth.js
        /// </summary>
        /// <param name="userId">User ID from NextAuth session</param>
        /// <returns>User data if valid</returns>
        [HttpGet("validate-user/{userId:guid}")]
        public async Task<IActionResult> ValidateUser(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                
                if (user == null || !user.IsActive)
                    return NotFound();

                var userDto = _mapper.Map<UserDto>(user);
                return Ok(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating user {UserId}", userId);
                return StatusCode(500, "An error occurred validating the user");
            }
        }

        /// <summary>
        /// Get user by email for NextAuth.js
        /// </summary>
        /// <param name="email">User email</param>
        /// <returns>User data if found</returns>
        [HttpGet("user-by-email")]
        public async Task<IActionResult> GetUserByEmail([FromQuery] string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                    return BadRequest("Email is required");

                var user = await _userRepository.GetByEmailAsync(email);
                
                if (user == null)
                    return NotFound();

                var userDto = _mapper.Map<UserDto>(user);
                return Ok(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by email {Email}", email);
                return StatusCode(500, "An error occurred retrieving the user");
            }
        }
    }

    public class GoogleOAuthRequest
    {
        public string Sub { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Image { get; set; }
        public bool EmailVerified { get; set; }
    }
} 