// Nullable reference types are used below
#nullable enable
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PolarNet.Models
{
    /// <summary>
    /// Subscription shape returned within customer state.
    /// </summary>
    public class PolarCustomerStateSubscription
    {
        [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
        [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
        [JsonPropertyName("product_id")] public string ProductId { get; set; } = string.Empty;
        [JsonPropertyName("amount")] public int? Amount { get; set; }
        [JsonPropertyName("currency")] public string? Currency { get; set; }
        [JsonPropertyName("recurring_interval")] public string? RecurringInterval { get; set; }
        [JsonPropertyName("current_period_start")] public DateTime? CurrentPeriodStart { get; set; }
        [JsonPropertyName("current_period_end")] public DateTime? CurrentPeriodEnd { get; set; }
        [JsonPropertyName("cancel_at_period_end")] public bool CancelAtPeriodEnd { get; set; }
        [JsonPropertyName("canceled_at")] public DateTime? CanceledAt { get; set; }
        [JsonPropertyName("started_at")] public DateTime? StartedAt { get; set; }
        [JsonPropertyName("ends_at")] public DateTime? EndsAt { get; set; }
        [JsonPropertyName("discount_id")] public string? DiscountId { get; set; }

        /// <summary>Meter usage entries associated with this subscription.</summary>
        [JsonPropertyName("meters")] public List<PolarCustomerStateMeterUsage> Meters { get; set; } = new();
    }
}
