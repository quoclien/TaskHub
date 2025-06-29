using System;
using System.Collections.Generic;

namespace TaskHub.Api.Shared.Models
{
    public class Board : BaseModel
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsPublic { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public ICollection<BoardMember> Members { get; set; } = new List<BoardMember>();
    }
}
