#if NETSTANDARD2_0 || NETSTANDARD2_1
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#else
#nullable enable
#endif

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PolarNet.Models.Resources
{
    /// <summary>
    /// Represents a webhook endpoint configuration in Polar.
    /// </summary>
    public class PolarWebhookEndpoint
    {
        /// <summary>
        /// Unique identifier for the webhook endpoint.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Creation timestamp (UTC).
        /// </summary>
        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// Last modification timestamp (UTC), if any.
        /// </summary>
        [JsonPropertyName("modified_at")]
        public DateTimeOffset? ModifiedAt { get; set; }

        /// <summary>
        /// The destination URL where Polar will send webhook events.
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Optional secret used to sign webhook payloads.
        /// </summary>
        [JsonPropertyName("secret")]
        public string? Secret { get; set; }

        /// <summary>
        /// List of event types this endpoint subscribes to.
        /// </summary>
        [JsonPropertyName("events")]
        public List<string> Events { get; set; } = new List<string>();

        /// <summary>
        /// Owning organization identifier, if applicable.
        /// </summary>
        [JsonPropertyName("organization_id")]
        public string? OrganizationId { get; set; }

        /// <summary>
        /// Owning user identifier, if applicable.
        /// </summary>
        [JsonPropertyName("user_id")]
        public string? UserId { get; set; }

        /// <summary>
        /// Payload format for webhooks (e.g., raw, json).
        /// </summary>
        [JsonPropertyName("format")]
        public string Format { get; set; } = "raw";
    }

    /// <summary>
    /// Request model for creating a webhook endpoint.
    /// </summary>
    public class CreateWebhookEndpointRequest
    {
        /// <summary>
        /// Destination URL for the webhook endpoint.
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Optional signing secret.
        /// </summary>
        [JsonPropertyName("secret")]
        public string? Secret { get; set; }

        /// <summary>
        /// Events to subscribe to.
        /// </summary>
        [JsonPropertyName("events")]
        public List<string> Events { get; set; } = new List<string>();

        /// <summary>
        /// Payload format preference.
        /// </summary>
        [JsonPropertyName("format")]
        public string Format { get; set; } = "raw";
    }

    /// <summary>
    /// Request model for updating a webhook endpoint.
    /// </summary>
    public class UpdateWebhookEndpointRequest
    {
        /// <summary>
        /// New destination URL, if changing.
        /// </summary>
        [JsonPropertyName("url")]
        public string? Url { get; set; }

        /// <summary>
        /// New signing secret, if rotating.
        /// </summary>
        [JsonPropertyName("secret")]
        public string? Secret { get; set; }

        /// <summary>
        /// Updated set of subscribed events.
        /// </summary>
        [JsonPropertyName("events")]
        public List<string>? Events { get; set; }

        /// <summary>
        /// New payload format.
        /// </summary>
        [JsonPropertyName("format")]
        public string? Format { get; set; }
    }
}