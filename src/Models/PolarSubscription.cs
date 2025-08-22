using Newtonsoft.Json;
using System;

namespace PolarNet.Models
{
    /// <summary>
    /// Represents a subscription relationship between a customer and a product/price.
    /// </summary>
    public class PolarSubscription
    {
        /// <summary>Unique subscription identifier.</summary>
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>Subscription status.</summary>
        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;

        /// <summary>Customer identifier.</summary>
        [JsonProperty("customer_id")]
        public string CustomerId { get; set; } = string.Empty;

        /// <summary>Product identifier.</summary>
        [JsonProperty("product_id")]
        public string ProductId { get; set; } = string.Empty;

        /// <summary>Price identifier on the product.</summary>
        [JsonProperty("price_id")]
        public string PriceId { get; set; } = string.Empty;

        /// <summary>Whether the subscription will cancel at period end.</summary>
        [JsonProperty("cancel_at_period_end")]
        public bool CancelAtPeriodEnd { get; set; }

        /// <summary>Current billing period start (UTC, optional).</summary>
        [JsonProperty("current_period_start")]
        public DateTime? CurrentPeriodStart { get; set; }

        /// <summary>Current billing period end (UTC, optional).</summary>
        [JsonProperty("current_period_end")]
        public DateTime? CurrentPeriodEnd { get; set; }

        /// <summary>Creation timestamp (UTC).</summary>
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>Last modification timestamp (UTC).</summary>
        [JsonProperty("modified_at")]
        public DateTime ModifiedAt { get; set; }
    }
}
