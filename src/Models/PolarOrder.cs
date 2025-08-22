using Newtonsoft.Json;
using System;

namespace PolarNet.Models
{
    /// <summary>
    /// Represents a finalized order in Polar.
    /// </summary>
    public class PolarOrder
    {
        /// <summary>Order identifier.</summary>
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>Customer identifier for the order.</summary>
        [JsonProperty("customer_id")]
        public string CustomerId { get; set; } = string.Empty;

        /// <summary>Product identifier.</summary>
        [JsonProperty("product_id")]
        public string ProductId { get; set; } = string.Empty;

        /// <summary>Price identifier used.</summary>
        [JsonProperty("product_price_id")]
        public string ProductPriceId { get; set; } = string.Empty;

        /// <summary>Related subscription identifier, if any.</summary>
        [JsonProperty("subscription_id")]
        public string SubscriptionId { get; set; } = string.Empty;

        /// <summary>Total amount in minor units.</summary>
        [JsonProperty("amount")]
        public int Amount { get; set; }

        /// <summary>ISO currency code.</summary>
        [JsonProperty("currency")]
        public string Currency { get; set; } = string.Empty;

        /// <summary>Creation timestamp (UTC).</summary>
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
