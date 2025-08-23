using System.Text.Json.Serialization;

namespace PolarNet.Models
{
    /// <summary>
    /// Postal billing address used in customer creation.
    /// </summary>
    public class PolarBillingAddress
    {
        [JsonPropertyName("line1")] public string? Line1 { get; set; }
        [JsonPropertyName("line2")] public string? Line2 { get; set; }
        [JsonPropertyName("postal_code")] public string? PostalCode { get; set; }
        [JsonPropertyName("city")] public string? City { get; set; }
        [JsonPropertyName("state")] public string? State { get; set; }
        [JsonPropertyName("country")] public string? Country { get; set; }
    }
}
