// Enable nullable for this file
#if NETSTANDARD2_0 || NETSTANDARD2_1
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#else
#nullable enable
#endif

using System;
using System.Text.Json.Serialization;

namespace PolarNet.Models
{
    /// <summary>
    /// Represents a customer in Polar.
    /// </summary>
    /// <remarks>
    /// A customer is an end-user who can purchase products, create subscriptions, and receive benefits.
    /// </remarks>
    public class PolarCustomer
    {
        /// <summary>
        /// Unique customer identifier.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Customer email address.
        /// </summary>
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Customer name.
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// Owning organization id.
        /// </summary>
        [JsonPropertyName("organization_id")]
        public string OrganizationId { get; set; } = string.Empty;

        /// <summary>
        /// Creation timestamp (UTC).
        /// </summary>
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Last modification timestamp (UTC).
        /// </summary>
        [JsonPropertyName("modified_at")]
        public DateTime? ModifiedAt { get; set; }
    }
}