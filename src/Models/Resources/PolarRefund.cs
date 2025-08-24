#if NETSTANDARD2_0 || NETSTANDARD2_1
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#else
#nullable enable
#endif

using System;
using System.Text.Json.Serialization;

namespace PolarNet.Models.Resources
{
    /// <summary>
    /// Represents a refund resource in Polar.
    /// </summary>
    public class PolarRefund
    {
        /// <summary>
        /// Unique identifier for the refund.
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
        /// Current refund status (e.g., pending, succeeded, failed, canceled).
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Reason for the refund.
        /// </summary>
        [JsonPropertyName("reason")]
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Optional comment or note associated with the refund.
        /// </summary>
        [JsonPropertyName("comment")]
        public string? Comment { get; set; }

        /// <summary>
        /// Refunded amount in the smallest currency unit.
        /// </summary>
        [JsonPropertyName("amount")]
        public long Amount { get; set; }

        /// <summary>
        /// Portion of the amount that represents taxes (smallest currency unit).
        /// </summary>
        [JsonPropertyName("tax_amount")]
        public long TaxAmount { get; set; }

        /// <summary>
        /// Three-letter ISO currency code.
        /// </summary>
        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;

        /// <summary>
        /// Identifier of the related order.
        /// </summary>
        [JsonPropertyName("order_id")]
        public string OrderId { get; set; } = string.Empty;

        /// <summary>
        /// Identifier of the related payment, if applicable.
        /// </summary>
        [JsonPropertyName("payment_id")]
        public string? PaymentId { get; set; }

        /// <summary>
        /// Identifier of the customer, if available.
        /// </summary>
        [JsonPropertyName("customer_id")]
        public string? CustomerId { get; set; }

        /// <summary>
        /// Identifier of the organization associated with this refund.
        /// </summary>
        [JsonPropertyName("organization_id")]
        public string OrganizationId { get; set; } = string.Empty;

        /// <summary>
        /// Arbitrary metadata attached to the refund.
        /// </summary>
        [JsonPropertyName("metadata")]
        public object? Metadata { get; set; }
    }
}