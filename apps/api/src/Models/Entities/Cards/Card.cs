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
        
        // Private field to store the JSON string
        private string _metadataJson = "{}";
        
        // Not mapped to database, used for object access
        [NotMapped]
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
        
        // Mapped to database, stores the serialized JSON
        [Column("Metadata")]
        public string MetadataJson
        {
            get => _metadataJson ?? JsonSerializer.Serialize(Metadata);
            set
            {
                _metadataJson = value;
                if (!string.IsNullOrEmpty(value))
                {
                    try
                    {
                        Metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(value) ?? new Dictionary<string, object>();
                    }
                    catch (JsonException)
                    {
                        Metadata = new Dictionary<string, object>();
                    }
                }
                else
                {
                    Metadata = new Dictionary<string, object>();
                }
            }
        }
        
        // Navigation properties
        public virtual required List List { get; set; }
    }
}
