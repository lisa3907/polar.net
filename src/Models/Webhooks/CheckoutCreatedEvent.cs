using System;
using System.Text.Json.Serialization;

namespace PolarNet.Models
{
    /// <summary>
    /// Checkout created event payload.
    /// </summary>
    /// <remarks>
    /// Emitted when a checkout session is created. The payload may be used to track checkout flows.
    /// </remarks>
    public class CheckoutCreatedEvent
    {
    /// <summary>Checkout session identifier.</summary>
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;

    /// <summary>Checkout status.</summary>
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;

    /// <summary>Customer identifier (if available).</summary>
    [JsonPropertyName("customer_id")] public string CustomerId { get; set; } = string.Empty;

    /// <summary>Product identifier.</summary>
    [JsonPropertyName("product_id")] public string ProductId { get; set; } = string.Empty;

    /// <summary>Success redirect URL.</summary>
    [JsonPropertyName("success_url")] public string SuccessUrl { get; set; } = string.Empty;

    /// <summary>Creation timestamp (UTC).</summary>
    [JsonPropertyName("created_at")] public DateTime CreatedAt { get; set; }
    }
}
