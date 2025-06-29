using System;

namespace TaskHub.Api.Shared.Models
{
    public class User : BaseModel
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Image { get; set; }
    }
}
