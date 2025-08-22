using System.Text.Json.Serialization;

namespace PolarNet.Models
{
    /// <summary>
    /// Request payload for creating a customer.
    /// </summary>
    /// <remarks>
    /// Sent to POST /v1/customers.
    /// </remarks>
    public class CreateCustomerRequest
    {
        /// <summary>Customer email (required).</summary>
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>Customer name (optional).</summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Owning organization id.</summary>
        [JsonPropertyName("organization_id")]
        public string OrganizationId { get; set; } = string.Empty;
    }
}
