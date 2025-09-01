using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace TaskHub.Api.Models.Dtos.Cards
{
    public class UpdateCardDto
    {
        [MaxLength(200)]
        public string? Title { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        public double? Position { get; set; }

        public Guid? ListId { get; set; }

        public Guid? AssignedTo { get; set; }

        public DateTime? DueDate { get; set; }

        public bool? IsArchived { get; set; }

        // Metadata as a JSON string that will be parsed into JsonDocument
        public string? Metadata { get; set; }

        // Helper method to get metadata as dictionary
        public Dictionary<string, object> GetMetadataAsDictionary()
        {
            if (string.IsNullOrEmpty(Metadata))
            {
                return new Dictionary<string, object>();
            }

            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, object>>(Metadata) ?? new Dictionary<string, object>();
            }
            catch (JsonException)
            {
                // If there's an error deserializing, return an empty dictionary
                return new Dictionary<string, object>();
            }
        }

        // Helper method to set metadata from dictionary
        public void SetMetadataFromDictionary(Dictionary<string, object> metadata)
        {
            Metadata = metadata != null ? JsonSerializer.Serialize(metadata) : null;
        }
    }
}
