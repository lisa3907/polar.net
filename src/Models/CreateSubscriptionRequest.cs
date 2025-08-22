using Newtonsoft.Json;

namespace PolarNet.Models
{
    /// <summary>
    /// Request payload for creating a subscription.
    /// </summary>
    public class CreateSubscriptionRequest
    {
        /// <summary>Customer identifier.</summary>
        [JsonProperty("customer_id")]
        public string CustomerId { get; set; } = string.Empty;

        /// <summary>Product price identifier.</summary>
        [JsonProperty("product_price_id")]
        public string ProductPriceId { get; set; } = string.Empty;
    }
}
