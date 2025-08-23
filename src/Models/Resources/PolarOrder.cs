using System;
using System.Text.Json.Serialization;

namespace PolarNet.Models
{
    /// <summary>
    /// Represents a finalized order in Polar.
    /// </summary>
    /// <remarks>
    /// Orders are created when a purchase completes and may link to a subscription.
    /// </remarks>
    public class PolarOrder
    {
        /// <summary>
        /// Order identifier.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Customer identifier for the order.
        /// </summary>
        [JsonPropertyName("customer_id")]
        public string CustomerId { get; set; } = string.Empty;

        /// <summary>
        /// Product identifier.
        /// </summary>
        [JsonPropertyName("product_id")]
        public string ProductId { get; set; } = string.Empty;

        /// <summary>
        /// Price identifier used.
        /// </summary>
        [JsonPropertyName("product_price_id")]
        public string ProductPriceId { get; set; } = string.Empty;

        /// <summary>
        /// Related subscription identifier, if any.
        /// </summary>
        [JsonPropertyName("subscription_id")]
        public string SubscriptionId { get; set; } = string.Empty;

        /// <summary>
        /// Total amount in minor units.
        /// </summary>
        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        /// <summary>
        /// ISO currency code.
        /// </summary>
        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;

        /// <summary>
        /// Creation timestamp (UTC).
        /// </summary>
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}