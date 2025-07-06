using System;
using System.Collections.Generic;
using TaskHub.Api.Models.Dtos.Comments;

namespace TaskHub.Api.Models.Dtos.Cards
{
    public class CardDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Position { get; set; }
        public Guid ListId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<CommentDto> Comments { get; set; } = new List<CommentDto>();
        public Dictionary<string, object>? Metadata { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsArchived { get; set; }
        public Guid? AssignedTo { get; set; }
    }
}
