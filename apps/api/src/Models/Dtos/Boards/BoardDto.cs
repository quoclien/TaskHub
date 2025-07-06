using System;
using System.Collections.Generic;
using TaskHub.Api.Models.Dtos.Lists;

namespace TaskHub.Api.Models.Dtos.Boards
{
    public class BoardDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsArchived { get; set; }
        public bool IsPublic { get; set; }
        public Guid OwnerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<BoardMemberDto> Members { get; set; } = new List<BoardMemberDto>();
        public ICollection<ListDto> Lists { get; set; } = new List<ListDto>();
    }
}
