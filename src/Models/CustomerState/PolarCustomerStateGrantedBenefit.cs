// Nullable reference types are used below
#if NETSTANDARD2_0 || NETSTANDARD2_1
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#else
#nullable enable
#endif

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
        /// <summary>
        /// Granted benefit entry identifier.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Underlying benefit identifier.
        /// </summary>
        [JsonPropertyName("benefit_id")]
        public string BenefitId { get; set; } = string.Empty;

        /// <summary>
        /// Type of benefit.
        /// </summary>
        [JsonPropertyName("benefit_type")]
        public string BenefitType { get; set; } = string.Empty;

        /// <summary>
        /// When the benefit was granted (UTC).
        /// </summary>
        [JsonPropertyName("granted_at")]
        public DateTime GrantedAt { get; set; }

        /// <summary>
        /// Optional provider-specific properties (e.g., account_id).
        /// </summary>
        [JsonPropertyName("properties")]
        public Dictionary<string, string>? Properties { get; set; }

        /// <summary>
        /// Optional benefit metadata bag.
        /// </summary>
        [JsonPropertyName("benefit_metadata")]
        public Dictionary<string, object>? BenefitMetadata { get; set; }
    }
}