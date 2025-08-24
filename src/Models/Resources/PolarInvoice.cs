// Enable nullable for this file
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
    /// Represents an invoice for an order.
    /// </summary>
    public class PolarInvoice
    {
        /// <summary>
        /// Unique identifier for the invoice.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Identifier of the order this invoice belongs to.
        /// </summary>
        [JsonPropertyName("order_id")]
        public string OrderId { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable invoice number or code.
        /// </summary>
        [JsonPropertyName("number")]
        public string Number { get; set; } = string.Empty;

        /// <summary>
        /// Three-letter ISO currency code.
        /// </summary>
        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;

        /// <summary>
        /// Total amount including taxes (smallest currency unit).
        /// </summary>
        [JsonPropertyName("amount_total")]
        public long AmountTotal { get; set; }

        /// <summary>
        /// Total tax amount (smallest currency unit).
        /// </summary>
        [JsonPropertyName("tax_amount")]
        public long TaxAmount { get; set; }

        /// <summary>
        /// Creation timestamp in UTC.
        /// </summary>
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Collection of line items included in the invoice.
        /// </summary>
        [JsonPropertyName("lines")]
        public List<PolarInvoiceLine> Lines { get; set; } = new();
    }

    /// <summary>
    /// A single line item on an invoice.
    /// </summary>
    public class PolarInvoiceLine
    {
        /// <summary>
        /// Description of the line item.
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Quantity of the item or unit count.
        /// </summary>
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        /// <summary>
        /// Price per unit in the smallest currency unit.
        /// </summary>
        [JsonPropertyName("unit_amount")]
        public long UnitAmount { get; set; }

        /// <summary>
        /// Extended amount for this line (quantity x unit_amount).
        /// </summary>
        [JsonPropertyName("amount")]
        public long Amount { get; set; }
    }
}
