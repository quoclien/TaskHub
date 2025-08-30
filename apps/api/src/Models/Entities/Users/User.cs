using System;
using System.Collections.Generic;
using TaskHub.Api.Models.Entities.Boards;
using TaskHub.Api.Models.Entities.Enums;

namespace TaskHub.Api.Models.Entities.Users
{
    public class User : BaseEntity
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PasswordHash { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? AvatarUrl { get; set; }
        public string? GoogleId { get; set; }
        public UserRole Role { get; set; } = UserRole.User;
        public bool IsActive { get; set; } = true;
        public DateTime? LastLoginAt { get; set; }

        // Navigation properties
        public virtual ICollection<Board> OwnedBoards { get; set; } = new List<Board>();
        public virtual ICollection<BoardMember> BoardMemberships { get; set; } = new List<BoardMember>();
    }
}
