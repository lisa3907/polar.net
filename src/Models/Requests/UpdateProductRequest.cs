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
    /// Request payload for updating a product.
    /// </summary>
    /// <remarks>
    /// Sent to PATCH /v1/products/{id}
    /// Only provided fields will be updated.
    /// </remarks>
    public class UpdateProductRequest
    {
    /// <summary>
    /// New human-readable product name.
    /// </summary>
        [JsonPropertyName("name")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Name { get; set; }

    /// <summary>
    /// Optional new description for the product.
    /// </summary>
        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }

    /// <summary>
    /// Whether the product becomes recurring. Omit to leave unchanged.
    /// </summary>
        [JsonPropertyName("is_recurring")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool IsRecurring { get; set; }

    /// <summary>
    /// New recurring interval when <see cref="IsRecurring"/> is true.
    /// Typical values: "day", "week", "month", "year".
    /// </summary>
        [JsonPropertyName("recurring_interval")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? RecurringInterval { get; set; }

    /// <summary>
    /// Metadata updates to apply to the product.
    /// </summary>
        [JsonPropertyName("metadata")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, string>? Metadata { get; set; }
    }
}
