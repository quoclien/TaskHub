using TaskHub.Api.Models.Dtos.Users;

namespace TaskHub.Api.Models.Dtos.Auth
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public UserDto User { get; set; } = null!;
    }
}