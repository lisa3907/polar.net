using Newtonsoft.Json;

namespace PolarNet.Models
{
    /// <summary>
    /// Request payload for creating a customer.
    /// </summary>
    public class CreateCustomerRequest
    {
        /// <summary>Customer email (required).</summary>
        [JsonProperty("email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>Customer name (optional).</summary>
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Owning organization id.</summary>
        [JsonProperty("organization_id")]
        public string OrganizationId { get; set; } = string.Empty;
    }
}
