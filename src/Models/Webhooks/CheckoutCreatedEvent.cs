using System;
using System.Text.Json.Serialization;

namespace PolarNet.Models
{
    /// <summary>
    /// Checkout created event payload.
    /// </summary>
    public class CheckoutCreatedEvent
    {
        [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
        [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
        [JsonPropertyName("customer_id")] public string CustomerId { get; set; } = string.Empty;
        [JsonPropertyName("product_id")] public string ProductId { get; set; } = string.Empty;
        [JsonPropertyName("success_url")] public string SuccessUrl { get; set; } = string.Empty;
        [JsonPropertyName("created_at")] public DateTime CreatedAt { get; set; }
    }
}
