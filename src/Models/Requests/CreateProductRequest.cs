// Enable nullable for this file
#if NETSTANDARD2_0 || NETSTANDARD2_1
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#else
#nullable enable
#endif

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PolarNet.Models
{
    /// <summary>
    /// Request payload for creating a product.
    /// </summary>
    /// <remarks>
    /// Sent to POST /v1/products
    /// Required fields depend on your pricing model. At minimum, provide a name.
    /// </remarks>
    public class CreateProductRequest
    {
        /// <summary>
        /// Human-readable product name.
        /// </summary>
        /// <example>Pro Plan</example>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Optional plain-text description of the product.
        /// </summary>
        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }

        /// <summary>
        /// Indicates whether this product is billed on a recurring basis.
        /// </summary>
        [JsonPropertyName("is_recurring")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool IsRecurring { get; set; }

        /// <summary>
        /// Recurring interval if <see cref="IsRecurring"/> is true.
        /// Typical values: "day", "week", "month", "year".
        /// </summary>
        [JsonPropertyName("recurring_interval")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? RecurringInterval { get; set; }

        /// <summary>
        /// Free-form key/value metadata to attach to the product.
        /// </summary>
        [JsonPropertyName("metadata")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, string>? Metadata { get; set; }
    }
}
