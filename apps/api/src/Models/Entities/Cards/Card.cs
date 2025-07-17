using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using TaskHub.Api.Models.Entities;
using TaskHub.Api.Models.Entities.Lists;

namespace TaskHub.Api.Models.Entities.Cards
{
    public class Card : BaseEntity
    {
        // Required properties with default values
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid ListId { get; set; }
        public int Position { get; set; }
        
        // Nullable properties
        public DateTime? DueDate { get; set; }
        public Guid? AssignedTo { get; set; }
        
        // Boolean with default value
        public bool IsArchived { get; set; }
        
        // JSONB metadata property for PostgreSQL
        public Dictionary<string, object>? Metadata { get; set; }
        
        // Navigation properties
        public virtual required List List { get; set; }
    }
}
