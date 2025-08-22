using System.Text.Json.Serialization;

namespace PolarNet.Models
{
    /// <summary>
    /// Represents a benefit granted by a product.
    /// </summary>
    /// <remarks>
    /// Benefits declare entitlements users receive when purchasing certain products or subscriptions.
    /// </remarks>
    public class PolarBenefit
    {
        /// <summary>Unique benefit identifier.</summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>Benefit name.</summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Benefit description.</summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>Benefit type.</summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>Owning organization id.</summary>
        [JsonPropertyName("organization_id")]
        public string OrganizationId { get; set; } = string.Empty;
    }
}
