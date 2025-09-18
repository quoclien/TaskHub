using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TaskHub.Api.Models.Dtos.Cards
{
    public class CreateCardDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        public double Position { get; set; }

        public Guid? AssignedTo { get; set; }

        public DateTime? DueDate { get; set; }
    }
}
