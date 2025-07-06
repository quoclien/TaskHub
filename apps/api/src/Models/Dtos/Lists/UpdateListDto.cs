using System.ComponentModel.DataAnnotations;

namespace TaskHub.Api.Models.Dtos.Lists
{
    public class UpdateListDto
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;
        
                public int? Position { get; set; }

        [Required]
        public Guid? BoardId { get; set; }
    }
}
