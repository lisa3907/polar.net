using System;
using System.Text.Json.Serialization;

namespace PolarNet.Models
{
    /// <summary>
    /// Subscription created event payload.
    /// </summary>
    /// <remarks>
    /// Emitted when a subscription is created. Fields mirror Polar's webhook schema.
    /// </remarks>
    public class SubscriptionCreatedEvent
    {
        /// <summary>
        /// Subscription identifier.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Subscription status.
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Customer identifier.
        /// </summary>
        [JsonPropertyName("customer_id")]
        public string CustomerId { get; set; } = string.Empty;

        /// <summary>
        /// Product identifier.
        /// </summary>
        [JsonPropertyName("product_id")]
        public string ProductId { get; set; } = string.Empty;

        /// <summary>
        /// Price identifier.
        /// </summary>
        [JsonPropertyName("price_id")]
        public string PriceId { get; set; } = string.Empty;

        /// <summary>
        /// Creation timestamp (UTC).
        /// </summary>
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}