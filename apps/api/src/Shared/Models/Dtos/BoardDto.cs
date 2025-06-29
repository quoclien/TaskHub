using System;
using System.Collections.Generic;

namespace TaskHub.Api.Shared.Models.Dtos
{
    public class BoardDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsPublic { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public ICollection<BoardMemberDto> Members { get; set; } = new List<BoardMemberDto>();
    }

    public class BoardMemberDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
    }
}
