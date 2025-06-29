using System;

namespace TaskHub.Api.Shared.Models
{
    public class Comment : BaseModel
    {
        public string Content { get; set; } = string.Empty;
        public string CardId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string? ParentId { get; set; }
    }
}
