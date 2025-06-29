using System;

namespace TaskHub.Api.Shared.Models
{
    public abstract class BaseModel
    {
        public string Id { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
