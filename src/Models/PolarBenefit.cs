using Newtonsoft.Json;

namespace PolarNet.Models
{
    /// <summary>
    /// Represents a benefit granted by a product.
    /// </summary>
    public class PolarBenefit
    {
        /// <summary>Unique benefit identifier.</summary>
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>Benefit name.</summary>
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Benefit description.</summary>
        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>Benefit type.</summary>
        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>Owning organization id.</summary>
        [JsonProperty("organization_id")]
        public string OrganizationId { get; set; } = string.Empty;
    }
}
