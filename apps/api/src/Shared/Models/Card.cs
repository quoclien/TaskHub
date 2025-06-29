using System;
using System.Collections.Generic;
using System.Text.Json;

namespace TaskHub.Api.Shared.Models
{
    public class Card : BaseModel
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Position { get; set; }
        public string ListId { get; set; } = string.Empty;
        public string BoardId { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public ICollection<string> AssignedTo { get; set; } = new List<string>();
        public DateTime? DueDate { get; set; }
        public ICollection<string> Labels { get; set; } = new List<string>();
        public ICollection<string> Attachments { get; set; } = new List<string>();
        public JsonDocument? Metadata { get; set; }
    }
}
