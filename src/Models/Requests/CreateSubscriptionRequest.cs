using System.Text.Json.Serialization;

namespace PolarNet.Models
{
    /// <summary>
    /// Request payload for creating a subscription.
    /// </summary>
    /// <remarks>
    /// Sent to POST /v1/subscriptions.
    /// </remarks>
    public class CreateSubscriptionRequest
    {
        /// <summary>
        /// Product price identifier.
        /// </summary>
        [JsonPropertyName("product_price_id")]
        public string ProductPriceId { get; set; } = string.Empty;
    }
}