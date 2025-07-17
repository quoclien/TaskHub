using TaskHub.Api.Models.Dtos.Auth;
using TaskHub.Api.Models.Dtos.Users;

namespace TaskHub.Api.Core.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<UserDto?> GetCurrentUserAsync(Guid userId);
        string GenerateJwtToken(Guid userId);
    }
} 