using Newtonsoft.Json;

namespace PolarNet.Models
{
    /// <summary>
    /// Represents a checkout session created in Polar.
    /// </summary>
    public class PolarCheckout
    {
        /// <summary>Checkout identifier.</summary>
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>Checkout status.</summary>
        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;

        /// <summary>Client secret value for client-side operations.</summary>
        [JsonProperty("client_secret")]
        public string ClientSecret { get; set; } = string.Empty;

        /// <summary>Hosted checkout URL.</summary>
        [JsonProperty("url")]
        public string Url { get; set; } = string.Empty;

        /// <summary>Customer identifier (if available).</summary>
        [JsonProperty("customer_id")]
        public string CustomerId { get; set; } = string.Empty;

        /// <summary>Product identifier.</summary>
        [JsonProperty("product_id")]
        public string ProductId { get; set; } = string.Empty;

        /// <summary>Product price identifier used to create this checkout.</summary>
        [JsonProperty("product_price_id")]
        public string ProductPriceId { get; set; } = string.Empty;

        /// <summary>Success redirect URL.</summary>
        [JsonProperty("success_url")]
        public string SuccessUrl { get; set; } = string.Empty;

        /// <summary>Allowed embed origin (if applicable).</summary>
        [JsonProperty("embed_origin")]
        public string EmbedOrigin { get; set; } = string.Empty;
    }
}
