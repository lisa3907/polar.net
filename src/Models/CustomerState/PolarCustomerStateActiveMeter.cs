using System.Text.Json.Serialization;

namespace PolarNet.Models
{
    /// <summary>
    /// Active meter shape in customer state.
    /// </summary>
    public class PolarCustomerStateActiveMeter
    {
    /// <summary>Active meter identifier.</summary>
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;

    /// <summary>Underlying meter definition identifier.</summary>
    [JsonPropertyName("meter_id")] public string MeterId { get; set; } = string.Empty;

    /// <summary>Total consumed units.</summary>
    [JsonPropertyName("consumed_units")] public int ConsumedUnits { get; set; }

    /// <summary>Total credited units.</summary>
    [JsonPropertyName("credited_units")] public int CreditedUnits { get; set; }

    /// <summary>Remaining balance (credited - consumed).</summary>
    [JsonPropertyName("balance")] public int Balance { get; set; }
    }
}
