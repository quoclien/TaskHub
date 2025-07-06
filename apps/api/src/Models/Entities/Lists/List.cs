using System;
using System.Collections.Generic;
using TaskHub.Api.Models.Entities;
using TaskHub.Api.Models.Entities.Boards;
using TaskHub.Api.Models.Entities.Cards;

namespace TaskHub.Api.Models.Entities.Lists
{
    public class List : BaseEntity
    {
        public required string Title { get; set; }
        public Guid BoardId { get; set; }
        public int Position { get; set; }
        
        // Navigation properties
        public virtual required Board Board { get; set; }
        public virtual ICollection<Card> Cards { get; set; } = new List<Card>();
    }
}
