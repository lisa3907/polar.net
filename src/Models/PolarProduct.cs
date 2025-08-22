using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace PolarNet.Models
{
    /// <summary>
    /// Represents a sellable product in Polar.
    /// </summary>
    /// <remarks>Contains pricing and benefits information.</remarks>
    public class PolarProduct
    {
        /// <summary>Unique product identifier.</summary>
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>Product name.</summary>
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Product description.</summary>
        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>Owning organization id.</summary>
        [JsonProperty("organization_id")]
        public string OrganizationId { get; set; } = string.Empty;

        /// <summary>Whether the product is recurring (subscription).</summary>
        [JsonProperty("is_recurring")]
        public bool IsRecurring { get; set; }

        /// <summary>Recurring interval if recurring, e.g., monthly.</summary>
        [JsonProperty("recurring_interval")]
        public string RecurringInterval { get; set; } = string.Empty;

        /// <summary>Whether the product is archived.</summary>
        [JsonProperty("is_archived")]
        public bool IsArchived { get; set; }

        /// <summary>Available prices for this product.</summary>
        [JsonProperty("prices")]
        public List<PolarPrice> Prices { get; set; } = new();

        /// <summary>Benefits granted by this product.</summary>
        [JsonProperty("benefits")]
        public List<PolarBenefit> Benefits { get; set; } = new();

        /// <summary>Creation timestamp (UTC).</summary>
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>Last modification timestamp (UTC).</summary>
        [JsonProperty("modified_at")]
        public DateTime ModifiedAt { get; set; }
    }
}
