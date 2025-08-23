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
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonPropertyName("modified_at")]
        public DateTimeOffset? ModifiedAt { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("secret")]
        public string? Secret { get; set; }

        [JsonPropertyName("events")]
        public List<string> Events { get; set; } = new List<string>();

        [JsonPropertyName("organization_id")]
        public string? OrganizationId { get; set; }

        [JsonPropertyName("user_id")]
        public string? UserId { get; set; }

        [JsonPropertyName("format")]
        public string Format { get; set; } = "raw";
    }

    /// <summary>
    /// Request model for creating a webhook endpoint.
    /// </summary>
    public class CreateWebhookEndpointRequest
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("secret")]
        public string? Secret { get; set; }

        [JsonPropertyName("events")]
        public List<string> Events { get; set; } = new List<string>();

        [JsonPropertyName("format")]
        public string Format { get; set; } = "raw";
    }

    /// <summary>
    /// Request model for updating a webhook endpoint.
    /// </summary>
    public class UpdateWebhookEndpointRequest
    {
        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("secret")]
        public string? Secret { get; set; }

        [JsonPropertyName("events")]
        public List<string>? Events { get; set; }

        [JsonPropertyName("format")]
        public string? Format { get; set; }
    }
}