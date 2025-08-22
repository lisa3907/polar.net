using System;
using System.Text.Json.Serialization;

namespace PolarNet.Models
{
    /// <summary>
    /// Customer created event payload.
    /// </summary>
    public class CustomerCreatedEvent
    {
        [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
        [JsonPropertyName("email")] public string Email { get; set; } = string.Empty;
        [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
        [JsonPropertyName("organization_id")] public string OrganizationId { get; set; } = string.Empty;
        [JsonPropertyName("created_at")] public DateTime CreatedAt { get; set; }
    }
}
