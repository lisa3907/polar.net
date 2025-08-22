using System.Text.Json.Serialization;

namespace PolarNet.Models
{
    /// <summary>
    /// Active meter shape in customer state.
    /// </summary>
    public class PolarCustomerStateActiveMeter
    {
        [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
        [JsonPropertyName("meter_id")] public string MeterId { get; set; } = string.Empty;
        [JsonPropertyName("consumed_units")] public int ConsumedUnits { get; set; }
        [JsonPropertyName("credited_units")] public int CreditedUnits { get; set; }
        [JsonPropertyName("balance")] public int Balance { get; set; }
    }
}
