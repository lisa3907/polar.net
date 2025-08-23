using System;
using System.Text.Json.Serialization;

namespace PolarNet.Models
{
    /// <summary>
    /// Customer created event payload.
    /// </summary>
    /// <remarks>
    /// Emitted when a new customer entity is created in Polar.
    /// </remarks>
    public class CustomerCreatedEvent
    {
        /// <summary>
        /// Customer identifier.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Customer email address.
        /// </summary>
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Customer name.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Owning organization identifier.
        /// </summary>
        [JsonPropertyName("organization_id")]
        public string OrganizationId { get; set; } = string.Empty;

        /// <summary>
        /// Creation timestamp (UTC).
        /// </summary>
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}