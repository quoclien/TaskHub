using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskHub.Api.Core.Exceptions;
using TaskHub.Api.Core.Interfaces;
using TaskHub.Api.Models.Dtos.Auth;
using TaskHub.Api.Models.Dtos.Users;

namespace TaskHub.Api.Controllers
{
    public class AuthController : ApiControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        /// <param name="registerDto">User registration data</param>
        /// <returns>Authentication response with JWT token</returns>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                var response = await _authService.RegisterAsync(registerDto);
                return CreatedAtAction(nameof(GetCurrentUser), new { }, response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Registration failed: {Message}", ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred during registration" });
            }
        }

        /// <summary>
        /// Authenticate a user
        /// </summary>
        /// <param name="loginDto">User login credentials</param>
        /// <returns>Authentication response with JWT token</returns>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var response = await _authService.LoginAsync(loginDto);
                return Ok(response);
            }
            catch (UnauthorizedException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user login");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred during login" });
            }
        }

        /// <summary>
        /// Get current authenticated user information
        /// </summary>
        /// <returns>Current user information</returns>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            try
            {
                var userId = Guid.Parse(User?.Identity?.Name ?? throw new UnauthorizedAccessException("User not authenticated"));
                var user = await _authService.GetCurrentUserAsync(userId);

                return user == null ? NotFound() : Ok(user);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current user");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving user information" });
            }
        }
    }
}