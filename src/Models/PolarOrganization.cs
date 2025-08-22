using Newtonsoft.Json;
using System;

namespace PolarNet.Models
{
    /// <summary>
    /// Represents an organization in Polar.
    /// </summary>
    /// <remarks>See Polar API: organizations.</remarks>
    public class PolarOrganization
    {
        /// <summary>Unique identifier of the organization.</summary>
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>Organization name.</summary>
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Slug of the organization.</summary>
        [JsonProperty("slug")]
        public string Slug { get; set; } = string.Empty;

        /// <summary>Contact email of the organization.</summary>
        [JsonProperty("email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>Website URL of the organization.</summary>
        [JsonProperty("website")]
        public string Website { get; set; } = string.Empty;

        /// <summary>When the organization was created (UTC).</summary>
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>When the organization was last modified (UTC).</summary>
        [JsonProperty("modified_at")]
        public DateTime ModifiedAt { get; set; }
    }
}
