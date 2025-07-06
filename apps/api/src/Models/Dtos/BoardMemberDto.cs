using TaskHub.Api.Models.Entities.Enums;

namespace TaskHub.Api.Models.Dtos
{
    public class BoardMemberDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string? UserAvatar { get; set; }
        public BoardRole Role { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}
