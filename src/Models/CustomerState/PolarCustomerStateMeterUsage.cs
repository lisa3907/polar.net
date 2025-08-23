using System.Text.Json.Serialization;

namespace PolarNet.Models
{
    /// <summary>
    /// Meter usage entry within a subscription in customer state.
    /// </summary>
    public class PolarCustomerStateMeterUsage
    {
        /// <summary>
        /// Meter usage entry identifier.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Meter definition identifier.
        /// </summary>
        [JsonPropertyName("meter_id")]
        public string MeterId { get; set; } = string.Empty;

        /// <summary>
        /// Units consumed in the current period.
        /// </summary>
        [JsonPropertyName("consumed_units")]
        public int ConsumedUnits { get; set; }

        /// <summary>
        /// Units credited in the current period.
        /// </summary>
        [JsonPropertyName("credited_units")]
        public int CreditedUnits { get; set; }

        /// <summary>
        /// Monetary amount associated, if applicable (minor units).
        /// </summary>
        [JsonPropertyName("amount")]
        public int Amount { get; set; }
    }
}