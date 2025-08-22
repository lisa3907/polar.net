using Newtonsoft.Json;

namespace PolarNet.Models
{
    /// <summary>
    /// Represents a price (one-time or recurring) for a product.
    /// </summary>
    public class PolarPrice
    {
        /// <summary>Unique price identifier.</summary>
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>Associated product identifier.</summary>
        [JsonProperty("product_id")]
        public string ProductId { get; set; } = string.Empty;

        /// <summary>Amount type, e.g., fixed.</summary>
        [JsonProperty("amount_type")]
        public string AmountType { get; set; } = string.Empty;

        /// <summary>Price type, e.g., one_time or recurring.</summary>
        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>Recurring interval for subscriptions (if applicable).</summary>
        [JsonProperty("recurring_interval")]
        public string RecurringInterval { get; set; } = string.Empty;

        /// <summary>Amount in minor units (e.g., cents). Optional.</summary>
        [JsonProperty("price_amount")]
        public int? PriceAmount { get; set; }

        /// <summary>ISO currency code (e.g., USD).</summary>
        [JsonProperty("price_currency")]
        public string PriceCurrency { get; set; } = string.Empty;

        /// <summary>Whether this price is archived.</summary>
        [JsonProperty("is_archived")]
        public bool IsArchived { get; set; }
    }
}
