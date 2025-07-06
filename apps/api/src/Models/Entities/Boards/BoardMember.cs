using System;
using TaskHub.Api.Models.Entities.Enums;
using TaskHub.Api.Models.Entities.Users;

namespace TaskHub.Api.Models.Entities.Boards
{
    public class BoardMember
    {
        public Guid UserId { get; set; }
        public Guid BoardId { get; set; }
        public BoardRole Role { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Board Board { get; set; } = null!;
    }
}
