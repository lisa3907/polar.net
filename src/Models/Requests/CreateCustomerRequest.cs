// Enable nullable for this file
#if NETSTANDARD2_0 || NETSTANDARD2_1
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#else
#nullable enable
#endif

using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace PolarNet.Models
{
    /// <summary>
    /// Request payload for creating a customer.
    /// </summary>
    /// <remarks>
    /// Sent to POST /v1/customers.
    /// API doc: https://docs.polar.sh/api-reference/customers/create
    /// </remarks>
    public class CreateCustomerRequest
    {
        /// <summary>
        /// Customer email (required).
        /// </summary>
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Customer name (optional).
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Optional external reference identifier.
        /// </summary>
        [JsonPropertyName("external_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ExternalId { get; set; }

        /// <summary>
        /// Arbitrary metadata.
        /// </summary>
        [JsonPropertyName("metadata")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, string>? Metadata { get; set; }

        /// <summary>
        /// Postal billing address.
        /// </summary>
        [JsonPropertyName("billing_address")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public PolarBillingAddress? BillingAddress { get; set; }

        /// <summary>
        /// Optional tax id array, e.g. ["911144442","us_ein"].
        /// </summary>
        [JsonPropertyName("tax_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? TaxId { get; set; }

        /// <summary>
        /// Owning organization id (omit when using an organization token).
        /// </summary>
        [JsonPropertyName("organization_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? OrganizationId { get; set; }
    }
}