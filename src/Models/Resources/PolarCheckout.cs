using System.Text.Json.Serialization;

namespace PolarNet.Models
{
    /// <summary>
    /// Represents a checkout session created in Polar.
    /// </summary>
    /// <remarks>
    /// Returned from checkout creation endpoints and retrieval calls. The <see cref="Url"/>
    /// points to a hosted page; <see cref="ClientSecret"/> may be used by client-side SDKs
    /// where applicable.
    /// </remarks>
    public class PolarCheckout
    {
        /// <summary>
        /// Checkout identifier.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Checkout status.
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Client secret value for client-side operations.
        /// </summary>
        [JsonPropertyName("client_secret")]
        public string ClientSecret { get; set; } = string.Empty;

        /// <summary>
        /// Hosted checkout URL.
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Customer identifier (if available).
        /// </summary>
        [JsonPropertyName("customer_id")]
        public string CustomerId { get; set; } = string.Empty;

        /// <summary>
        /// Product identifier.
        /// </summary>
        [JsonPropertyName("product_id")]
        public string ProductId { get; set; } = string.Empty;

        /// <summary>
        /// Product price identifier used to create this checkout.
        /// </summary>
        [JsonPropertyName("product_price_id")]
        public string ProductPriceId { get; set; } = string.Empty;

        /// <summary>
        /// Success redirect URL.
        /// </summary>
        [JsonPropertyName("success_url")]
        public string SuccessUrl { get; set; } = string.Empty;

        /// <summary>
        /// Allowed embed origin (if applicable).
        /// </summary>
        [JsonPropertyName("embed_origin")]
        public string EmbedOrigin { get; set; } = string.Empty;
    }
}