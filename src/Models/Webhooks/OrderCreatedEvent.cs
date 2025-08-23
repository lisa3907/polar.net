using System;
using System.Text.Json.Serialization;

namespace PolarNet.Models
{
    /// <summary>
    /// Order created event payload.
    /// </summary>
    /// <remarks>
    /// Emitted after a successful purchase. Use this event to provision access or record sales.
    /// </remarks>
    public class OrderCreatedEvent
    {
        /// <summary>
        /// Order identifier.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

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
        /// Order status.
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Creation timestamp (UTC).
        /// </summary>
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}