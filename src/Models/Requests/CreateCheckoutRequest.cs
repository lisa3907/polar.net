using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace PolarNet.Models
{
    /// <summary>
    /// Request payload for creating a custom checkout session.
    /// </summary>
    /// <remarks>
    /// Sent to POST /v1/checkouts/custom.
    /// </remarks>
    public class CreateCheckoutRequest
    {
        /// <summary>Product price identifier (required).</summary>
        [JsonPropertyName("product_price_id")]
        public string ProductPriceId { get; set; } = string.Empty;

        /// <summary>Success redirect URL.</summary>
        [JsonPropertyName("success_url")]
        public string SuccessUrl { get; set; } = string.Empty;

        /// <summary>Customer email if known.</summary>
        [JsonPropertyName("customer_email")]
        public string CustomerEmail { get; set; } = string.Empty;

        /// <summary>Arbitrary metadata to attach to the checkout.</summary>
        [JsonPropertyName("metadata")]
        public Dictionary<string, string> Metadata { get; set; } = new();
    }
}
