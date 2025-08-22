using System.Text.Json.Serialization;

namespace PolarNet.Models
{
    /// <summary>
    /// Meter usage entry within a subscription in customer state.
    /// </summary>
    public class PolarCustomerStateMeterUsage
    {
        [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
        [JsonPropertyName("meter_id")] public string MeterId { get; set; } = string.Empty;
        [JsonPropertyName("consumed_units")] public int ConsumedUnits { get; set; }
        [JsonPropertyName("credited_units")] public int CreditedUnits { get; set; }
        [JsonPropertyName("amount")] public int Amount { get; set; }
    }
}
