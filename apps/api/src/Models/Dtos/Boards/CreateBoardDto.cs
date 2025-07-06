using System.ComponentModel.DataAnnotations;

namespace TaskHub.Api.Models.Dtos.Boards
{
    public class CreateBoardDto
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        public bool IsPublic { get; set; } = false;
    }
}
