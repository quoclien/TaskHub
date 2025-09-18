using System.ComponentModel.DataAnnotations;

namespace TaskHub.Api.Models.Dtos.Boards
{
    public class UpdateBoardDto
    {
        [MaxLength(100)]
        public string? Title { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool? IsArchived { get; set; }

        public bool? IsPublic { get; set; }
    }
}
