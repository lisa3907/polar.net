using System;
using System.Text.Json.Serialization;

namespace PolarNet.Models
{
    /// <summary>
    /// Subscription created event payload.
    /// </summary>
    public class SubscriptionCreatedEvent
    {
        [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
        [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
        [JsonPropertyName("customer_id")] public string CustomerId { get; set; } = string.Empty;
        [JsonPropertyName("product_id")] public string ProductId { get; set; } = string.Empty;
        [JsonPropertyName("price_id")] public string PriceId { get; set; } = string.Empty;
        [JsonPropertyName("created_at")] public DateTime CreatedAt { get; set; }
    }
}
