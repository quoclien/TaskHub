using System;

namespace TaskHub.Api.Shared.Models
{
    public class BoardMember
    {
        public string UserId { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
