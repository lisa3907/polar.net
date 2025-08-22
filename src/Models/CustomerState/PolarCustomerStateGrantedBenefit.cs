// Nullable reference types are used below
#nullable enable
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PolarNet.Models
{
    /// <summary>
    /// Granted benefit shape in customer state.
    /// </summary>
    public class PolarCustomerStateGrantedBenefit
    {
        [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
        [JsonPropertyName("benefit_id")] public string BenefitId { get; set; } = string.Empty;
        [JsonPropertyName("benefit_type")] public string BenefitType { get; set; } = string.Empty;
        [JsonPropertyName("granted_at")] public DateTime GrantedAt { get; set; }

        /// <summary>Optional provider-specific properties (e.g., account_id).</summary>
        [JsonPropertyName("properties")] public Dictionary<string, string>? Properties { get; set; }

        /// <summary>Optional benefit metadata bag.</summary>
        [JsonPropertyName("benefit_metadata")] public Dictionary<string, object>? BenefitMetadata { get; set; }
    }
}
