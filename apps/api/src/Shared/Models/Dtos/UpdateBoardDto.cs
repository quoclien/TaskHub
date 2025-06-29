using System.ComponentModel.DataAnnotations;

namespace TaskHub.Api.Shared.Models.Dtos
{
    public class UpdateBoardDto
    {
        [StringLength(100, MinimumLength = 3)]
        public string? Title { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public bool? IsPublic { get; set; }
    }
}
