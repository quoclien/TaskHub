using System.ComponentModel.DataAnnotations;

namespace TaskHub.Api.Models.Dtos.Lists
{
    public class UpdateListDto
    {
        [MaxLength(100)]
        public string? Title { get; set; }
        
        public int? Position { get; set; }

        public Guid? BoardId { get; set; }
    }
}
