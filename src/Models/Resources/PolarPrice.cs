using System.Text.Json.Serialization;

namespace PolarNet.Models
{
    /// <summary>
    /// Represents a price (one-time or recurring) for a product.
    /// </summary>
    /// <remarks>
    /// A price belongs to a product and can be one-time or recurring. For recurring prices,
    /// <c>recurring_interval</c> indicates the billing cadence. When <c>price_amount</c> is set,
    /// it's expressed in minor currency units (e.g., cents).
    /// </remarks>
    public class PolarPrice
    {
        /// <summary>Unique price identifier.</summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>Associated product identifier.</summary>
        [JsonPropertyName("product_id")]
        public string ProductId { get; set; } = string.Empty;

        /// <summary>Amount type, e.g., fixed.</summary>
        [JsonPropertyName("amount_type")]
        public string AmountType { get; set; } = string.Empty;

        /// <summary>Price type, e.g., one_time or recurring.</summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>Recurring interval for subscriptions (if applicable).</summary>
        [JsonPropertyName("recurring_interval")]
        public string RecurringInterval { get; set; } = string.Empty;

        /// <summary>Amount in minor units (e.g., cents). Optional.</summary>
        [JsonPropertyName("price_amount")]
        public int? PriceAmount { get; set; }

        /// <summary>ISO currency code (e.g., USD).</summary>
        [JsonPropertyName("price_currency")]
        public string PriceCurrency { get; set; } = string.Empty;

        /// <summary>Whether this price is archived.</summary>
        [JsonPropertyName("is_archived")]
        public bool IsArchived { get; set; }
    }
}
