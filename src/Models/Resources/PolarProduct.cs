using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PolarNet.Models
{
    /// <summary>
    /// Represents a sellable product in Polar.
    /// </summary>
    /// <remarks>Contains pricing and benefits information.</remarks>
    public class PolarProduct
    {
        /// <summary>Unique product identifier.</summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>Product name.</summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Product description.</summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>Owning organization id.</summary>
        [JsonPropertyName("organization_id")]
        public string OrganizationId { get; set; } = string.Empty;

        /// <summary>Whether the product is recurring (subscription).</summary>
        [JsonPropertyName("is_recurring")]
        public bool IsRecurring { get; set; }

        /// <summary>Recurring interval if recurring, e.g., monthly.</summary>
        [JsonPropertyName("recurring_interval")]
        public string RecurringInterval { get; set; } = string.Empty;

        /// <summary>Whether the product is archived.</summary>
        [JsonPropertyName("is_archived")]
        public bool IsArchived { get; set; }

        /// <summary>Available prices for this product.</summary>
        [JsonPropertyName("prices")]
        public List<PolarPrice> Prices { get; set; } = new();

        /// <summary>Benefits granted by this product.</summary>
        [JsonPropertyName("benefits")]
        public List<PolarBenefit> Benefits { get; set; } = new();

        /// <summary>Creation timestamp (UTC).</summary>
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>Last modification timestamp (UTC).</summary>
        [JsonPropertyName("modified_at")]
        public DateTime ModifiedAt { get; set; }
    }
}
