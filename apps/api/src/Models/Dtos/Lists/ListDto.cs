using System;
using System.Collections.Generic;
using TaskHub.Api.Models.Dtos.Cards;

namespace TaskHub.Api.Models.Dtos.Lists
{
    public class ListDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Position { get; set; }
        public Guid BoardId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<CardDto> Cards { get; set; } = new List<CardDto>();
    }
}
