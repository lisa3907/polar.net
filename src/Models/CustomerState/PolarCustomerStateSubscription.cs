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
    /// <summary>Subscription identifier.</summary>
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;

    /// <summary>Subscription status.</summary>
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;

    /// <summary>Associated product identifier.</summary>
    [JsonPropertyName("product_id")] public string ProductId { get; set; } = string.Empty;

    /// <summary>Amount in minor currency units (optional).</summary>
    [JsonPropertyName("amount")] public int? Amount { get; set; }

    /// <summary>ISO currency code (optional).</summary>
    [JsonPropertyName("currency")] public string? Currency { get; set; }

    /// <summary>Recurring interval (optional), e.g., monthly.</summary>
    [JsonPropertyName("recurring_interval")] public string? RecurringInterval { get; set; }

    /// <summary>Current period start timestamp (UTC, optional).</summary>
    [JsonPropertyName("current_period_start")] public DateTime? CurrentPeriodStart { get; set; }

    /// <summary>Current period end timestamp (UTC, optional).</summary>
    [JsonPropertyName("current_period_end")] public DateTime? CurrentPeriodEnd { get; set; }

    /// <summary>Whether cancellation at period end is scheduled.</summary>
    [JsonPropertyName("cancel_at_period_end")] public bool CancelAtPeriodEnd { get; set; }

    /// <summary>When the subscription was canceled (UTC, optional).</summary>
    [JsonPropertyName("canceled_at")] public DateTime? CanceledAt { get; set; }

    /// <summary>When the subscription started (UTC, optional).</summary>
    [JsonPropertyName("started_at")] public DateTime? StartedAt { get; set; }

    /// <summary>When the subscription ends (UTC, optional).</summary>
    [JsonPropertyName("ends_at")] public DateTime? EndsAt { get; set; }

    /// <summary>Applied discount identifier (optional).</summary>
    [JsonPropertyName("discount_id")] public string? DiscountId { get; set; }

    /// <summary>Meter usage entries associated with this subscription.</summary>
        [JsonPropertyName("meters")] public List<PolarCustomerStateMeterUsage> Meters { get; set; } = new();
    }
}
