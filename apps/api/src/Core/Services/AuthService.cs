using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using TaskHub.Api.Core.Exceptions;
using TaskHub.Api.Core.Interfaces;
using TaskHub.Api.Core.Interfaces.Repositories;
using TaskHub.Api.Models.Dtos.Auth;
using TaskHub.Api.Models.Dtos.Users;
using TaskHub.Api.Models.Entities.Users;

namespace TaskHub.Api.Core.Services
{
    public class AuthService : BaseService, IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthService> _logger;
        private readonly IConfiguration _configuration;

        public AuthService(
            IUserRepository userRepository,
            IMapper mapper,
            ILogger<AuthService> logger,
            IConfiguration configuration,
            IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            if (registerDto == null)
                throw new ArgumentNullException(nameof(registerDto));

            // Check if email already exists
            if (await _userRepository.EmailExistsAsync(registerDto.Email))
                throw new InvalidOperationException("A user with this email already exists.");

            // Check if username already exists
            if (await _userRepository.UsernameExistsAsync(registerDto.Username))
                throw new InvalidOperationException("A user with this username already exists.");

            try
            {
                // Create new user
                var user = new User
                {
                    Username = registerDto.Username,
                    Email = registerDto.Email,
                    PasswordHash = HashPassword(registerDto.Password),
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _userRepository.AddAsync(user);
                await SaveChangesAsync();

                _logger.LogInformation("User {Email} registered successfully", registerDto.Email);

                // Generate token
                var token = GenerateJwtToken(user.Id);
                var userDto = _mapper.Map<UserDto>(user);

                return new AuthResponseDto
                {
                    Token = token,
                    ExpiresAt = GetTokenExpiryDate(),
                    User = userDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user {Email}", registerDto.Email);
                throw new ApplicationException("An error occurred while registering the user.", ex);
            }
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            if (loginDto == null)
                throw new ArgumentNullException(nameof(loginDto));

            try
            {
                // Find user by email or username
                var user = await FindUserByEmailOrUsernameAsync(loginDto.EmailOrUsername);

                // Check if user exists and has a password hash
                if (user == null || string.IsNullOrEmpty(user.PasswordHash) || !VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Failed login attempt for {EmailOrUsername}", loginDto.EmailOrUsername);
                    throw new UnauthorizedException("Invalid email/username or password.");
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("Login attempt for inactive user {UserId}", user.Id);
                    throw new UnauthorizedException("User account is inactive.");
                }

                // Update last login time
                user.LastLoginAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
                _userRepository.Update(user);
                await SaveChangesAsync();

                _logger.LogInformation("User {UserId} logged in successfully", user.Id);

                // Generate token
                var token = GenerateJwtToken(user.Id);
                var userDto = _mapper.Map<UserDto>(user);

                return new AuthResponseDto
                {
                    Token = token,
                    ExpiresAt = GetTokenExpiryDate(),
                    User = userDto
                };
            }
            catch (UnauthorizedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {EmailOrUsername}", loginDto.EmailOrUsername);
                throw new ApplicationException("An error occurred during login.", ex);
            }
        }

        public async Task<UserDto?> GetCurrentUserAsync(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                return user != null ? _mapper.Map<UserDto>(user) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user {UserId}", userId);
                throw new ApplicationException("An error occurred while retrieving user information.", ex);
            }
        }

        public string GenerateJwtToken(Guid userId)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key not configured")));

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, userId.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: GetTokenExpiryDate(),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }

        private async Task<User?> FindUserByEmailOrUsernameAsync(string emailOrUsername)
        {
            // Try to find by email first
            var user = await _userRepository.GetByEmailAsync(emailOrUsername);

            // If not found by email, try by username
            if (user == null)
            {
                var users = await _userRepository.FindAsync(u => u.Username == emailOrUsername);
                user = users.FirstOrDefault();
            }

            return user;
        }

        private string HashPassword(string password)
        {
            // Using BCrypt for password hashing
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrEmpty(hash))
                return false;

            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch
            {
                return false;
            }
        }

        private DateTime GetTokenExpiryDate()
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var expireDays = Convert.ToDouble(jwtSettings["ExpireDays"] ?? "7");
            return DateTime.UtcNow.AddDays(expireDays);
        }
    }
}