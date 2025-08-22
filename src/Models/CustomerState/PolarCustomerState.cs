// Enable nullable reference types locally for this file
#nullable enable

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PolarNet.Models
{
    /// <summary>
    /// Aggregated state for a customer, including active subscriptions, granted benefits and meters.
    /// </summary>
    public class PolarCustomerState
    {
        [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
        [JsonPropertyName("email")] public string Email { get; set; } = string.Empty;
        [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
        [JsonPropertyName("organization_id")] public string OrganizationId { get; set; } = string.Empty;
        [JsonPropertyName("avatar_url")] public string? AvatarUrl { get; set; }

        /// <summary>Active subscriptions for the customer.</summary>
        [JsonPropertyName("active_subscriptions")] public List<PolarCustomerStateSubscription> ActiveSubscriptions { get; set; } = new();

        /// <summary>Granted benefits for the customer.</summary>
        [JsonPropertyName("granted_benefits")] public List<PolarCustomerStateGrantedBenefit> GrantedBenefits { get; set; } = new();

        /// <summary>Active meters for the customer.</summary>
        [JsonPropertyName("active_meters")] public List<PolarCustomerStateActiveMeter> ActiveMeters { get; set; } = new();
    }
}
