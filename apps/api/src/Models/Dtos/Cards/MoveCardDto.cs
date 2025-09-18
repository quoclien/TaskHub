using System;
using System.ComponentModel.DataAnnotations;

namespace TaskHub.Api.Models.Dtos.Cards
{
    public class MoveCardDto
    {
        [Required]
        public Guid TargetListId { get; set; }

        [Required]
        public double NewPosition { get; set; }
    }
}
