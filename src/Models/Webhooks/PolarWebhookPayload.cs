using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PolarNet.Models
{
    /// <summary>
    /// Envelope for Polar webhook events.
    /// </summary>
    /// <remarks>
    /// The <see cref="Data"/> property contains the event-specific payload as a JsonElement.
    /// </remarks>
    public class PolarWebhookPayload
    {
        [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;
        [JsonPropertyName("data")] public JsonElement Data { get; set; }
        [JsonPropertyName("event_id")] public string EventId { get; set; } = string.Empty;
        [JsonPropertyName("created_at")] public DateTime CreatedAt { get; set; }
    }
}
