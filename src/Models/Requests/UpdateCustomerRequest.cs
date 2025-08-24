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
    /// Request payload for updating a customer.
    /// </summary>
    /// <remarks>
    /// Sent to PATCH /v1/customers/{id}
    /// Only provided fields will be updated.
    /// </remarks>
    public class UpdateCustomerRequest
    {
        /// <summary>
        /// New email address for the customer.
        /// </summary>
        [JsonPropertyName("email")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Email { get; set; }

        /// <summary>
        /// New display name for the customer.
        /// </summary>
        [JsonPropertyName("name")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Name { get; set; }

        /// <summary>
        /// External identifier from your system to correlate the customer.
        /// </summary>
        [JsonPropertyName("external_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ExternalId { get; set; }

        /// <summary>
        /// Free-form key/value metadata updates.
        /// </summary>
        [JsonPropertyName("metadata")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, string>? Metadata { get; set; }

        /// <summary>
        /// Updated billing address information.
        /// </summary>
        [JsonPropertyName("billing_address")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public PolarBillingAddress? BillingAddress { get; set; }

        /// <summary>
        /// Optional list of tax IDs associated with the customer.
        /// </summary>
        [JsonPropertyName("tax_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? TaxId { get; set; }
    }
}
