using System;
using System.Text.Json.Serialization;

namespace PolarNet.Models
{
    /// <summary>
    /// Order created event payload.
    /// </summary>
    public class OrderCreatedEvent
    {
        [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
        [JsonPropertyName("customer_id")] public string CustomerId { get; set; } = string.Empty;
        [JsonPropertyName("product_id")] public string ProductId { get; set; } = string.Empty;
        [JsonPropertyName("amount")] public int Amount { get; set; }
        [JsonPropertyName("currency")] public string Currency { get; set; } = string.Empty;
        [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
        [JsonPropertyName("created_at")] public DateTime CreatedAt { get; set; }
    }
}
