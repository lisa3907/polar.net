// Enable nullable for this file
#if NETSTANDARD2_0 || NETSTANDARD2_1
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#else
#nullable enable
#endif

using System.Text.Json.Serialization;

namespace PolarNet.Models
{
    /// <summary>
    /// Postal billing address used in customer creation.
    /// </summary>
    public class PolarBillingAddress
    {
        /// <summary>
        /// First address line (e.g., street name and number).
        /// </summary>
        [JsonPropertyName("line1")]
        public string? Line1 { get; set; }

        /// <summary>
        /// Second address line (e.g., apartment, suite, unit). Optional.
        /// </summary>
        [JsonPropertyName("line2")]
        public string? Line2 { get; set; }

        /// <summary>
        /// ZIP or postal code. Optional; format depends on the destination country.
        /// </summary>
        [JsonPropertyName("postal_code")]
        public string? PostalCode { get; set; }

        /// <summary>
        /// City or locality. Optional.
        /// </summary>
        [JsonPropertyName("city")]
        public string? City { get; set; }

        /// <summary>
        /// State, province, or region. Optional; use short codes when applicable (e.g., CA, NY).
        /// </summary>
        [JsonPropertyName("state")]
        public string? State { get; set; }

        /// <summary>
        /// Country code. Prefer ISO 3166-1 alpha-2 (e.g., US, GB, KR). Optional.
        /// </summary>
        [JsonPropertyName("country")]
        public string? Country { get; set; }
    }
}