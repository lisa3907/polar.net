using Newtonsoft.Json;
using System.Collections.Generic;

namespace PolarNet.Models
{
    /// <summary>
    /// Request payload for creating a custom checkout session.
    /// </summary>
    public class CreateCheckoutRequest
    {
        /// <summary>Product price identifier (required).</summary>
        [JsonProperty("product_price_id")]
        public string ProductPriceId { get; set; } = string.Empty;

        /// <summary>Success redirect URL.</summary>
        [JsonProperty("success_url")]
        public string SuccessUrl { get; set; } = string.Empty;

        /// <summary>Customer email if known.</summary>
        [JsonProperty("customer_email")]
        public string CustomerEmail { get; set; } = string.Empty;

        /// <summary>Arbitrary metadata to attach to the checkout.</summary>
        [JsonProperty("metadata")]
        public Dictionary<string, string> Metadata { get; set; } = new();
    }
}
