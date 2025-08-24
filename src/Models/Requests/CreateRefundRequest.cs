#if NETSTANDARD2_0 || NETSTANDARD2_1
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#else
#nullable enable
#endif

using System.Text.Json.Serialization;

namespace PolarNet.Models
{
    /// <summary>
    /// Request model for creating a refund.
    /// </summary>
    public class CreateRefundRequest
    {
        /// <summary>
        /// The associated order identifier to refund.
        /// </summary>
        [JsonPropertyName("order_id")]
        public string OrderId { get; set; } = string.Empty;

        /// <summary>
        /// Free-text reason for the refund.
        /// </summary>
        [JsonPropertyName("reason")]
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Optional operator comment or note for the refund.
        /// </summary>
        [JsonPropertyName("comment")]
        public string? Comment { get; set; }

        /// <summary>
        /// Optional amount to refund in the smallest currency unit. If omitted, a full refund may be processed depending on API semantics.
        /// </summary>
        [JsonPropertyName("amount")]
        public long? Amount { get; set; }
    }
}