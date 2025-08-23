using System;
using System.Text.Json.Serialization;

namespace PolarNet.Models
{
    /// <summary>
    /// Represents an organization in Polar.
    /// </summary>
    /// <remarks>See Polar API: organizations.</remarks>
    public class PolarOrganization
    {
        /// <summary>
        /// Unique identifier of the organization.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Organization name.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Slug of the organization.
        /// </summary>
        [JsonPropertyName("slug")]
        public string Slug { get; set; } = string.Empty;

        /// <summary>
        /// Contact email of the organization.
        /// </summary>
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Website URL of the organization.
        /// </summary>
        [JsonPropertyName("website")]
        public string Website { get; set; } = string.Empty;

        /// <summary>
        /// When the organization was created (UTC).
        /// </summary>
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// When the organization was last modified (UTC).
        /// </summary>
        [JsonPropertyName("modified_at")]
        public DateTime ModifiedAt { get; set; }
    }
}