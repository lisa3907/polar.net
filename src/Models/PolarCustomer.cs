using Newtonsoft.Json;
using System;

namespace PolarNet.Models
{
    /// <summary>
    /// Represents a customer in Polar.
    /// </summary>
    public class PolarCustomer
    {
        /// <summary>Unique customer identifier.</summary>
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>Customer email address.</summary>
        [JsonProperty("email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>Customer name.</summary>
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Owning organization id.</summary>
        [JsonProperty("organization_id")]
        public string OrganizationId { get; set; } = string.Empty;

        /// <summary>Creation timestamp (UTC).</summary>
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>Last modification timestamp (UTC).</summary>
        [JsonProperty("modified_at")]
        public DateTime ModifiedAt { get; set; }
    }
}
