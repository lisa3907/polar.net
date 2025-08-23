#if NETSTANDARD2_0 || NETSTANDARD2_1
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#else
#nullable enable
#endif

using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PolarNet.Models;
using PolarNet.Models.Resources;

namespace PolarNet.Services
{
    /// <summary>
    /// Refund-related endpoints.
    /// </summary>
    public sealed partial class PolarClient
    {
        /// <summary>
        /// Creates a refund for an order.
        /// </summary>
        /// <param name="request">Refund creation request with order ID and reason.</param>
        /// <returns>The created <see cref="PolarRefund"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
        /// <exception cref="Exception">Thrown with response details when the API returns a non-success status code.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarRefund> CreateRefundAsync(CreateRefundRequest request)
        {
            if (request is null) 
                throw new ArgumentNullException(nameof(request));

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await SendAsync(HttpMethod.Post, "/v1/refunds", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to create refund: {response.StatusCode} - {error}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarRefund>(responseContent)
                   ?? throw new InvalidOperationException("Failed to deserialize PolarRefund");
        }

        /// <summary>
        /// Creates a full refund for an order.
        /// </summary>
        /// <param name="orderId">Order identifier to refund.</param>
        /// <param name="reason">Reason for the refund (e.g., "duplicate", "fraudulent", "requested_by_customer", "other").</param>
        /// <param name="comment">Optional comment explaining the refund.</param>
        /// <returns>The created <see cref="PolarRefund"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="orderId"/> or <paramref name="reason"/> is empty.</exception>
        /// <exception cref="Exception">Thrown with response details when the API returns a non-success status code.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarRefund> CreateRefundAsync(string orderId, string reason, string? comment = null)
        {
            if (string.IsNullOrWhiteSpace(orderId)) 
                throw new ArgumentException("orderId is required", nameof(orderId));
            if (string.IsNullOrWhiteSpace(reason)) 
                throw new ArgumentException("reason is required", nameof(reason));

            var request = new CreateRefundRequest
            {
                OrderId = orderId,
                Reason = reason,
                Comment = comment
            };

            return await CreateRefundAsync(request);
        }

        /// <summary>
        /// Creates a partial refund for an order.
        /// </summary>
        /// <param name="orderId">Order identifier to refund.</param>
        /// <param name="amount">Amount to refund in the smallest currency unit (e.g., cents).</param>
        /// <param name="reason">Reason for the refund.</param>
        /// <param name="comment">Optional comment explaining the refund.</param>
        /// <returns>The created <see cref="PolarRefund"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when parameters are invalid.</exception>
        /// <exception cref="Exception">Thrown with response details when the API returns a non-success status code.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarRefund> CreatePartialRefundAsync(string orderId, long amount, string reason, string? comment = null)
        {
            if (string.IsNullOrWhiteSpace(orderId)) 
                throw new ArgumentException("orderId is required", nameof(orderId));
            if (string.IsNullOrWhiteSpace(reason)) 
                throw new ArgumentException("reason is required", nameof(reason));
            if (amount <= 0) 
                throw new ArgumentException("amount must be positive", nameof(amount));

            var request = new CreateRefundRequest
            {
                OrderId = orderId,
                Amount = amount,
                Reason = reason,
                Comment = comment
            };

            return await CreateRefundAsync(request);
        }

        /// <summary>
        /// Lists refunds for the configured organization.
        /// </summary>
        /// <param name="page">1-based page number. Defaults to 1.</param>
        /// <param name="limit">Items per page (1-100). Defaults to 10.</param>
        /// <returns>Paged list of refunds.</returns>
        /// <exception cref="HttpRequestException">Thrown when the HTTP request fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarListResponse<PolarRefund>> ListRefundsAsync(int page = 1, int limit = 10)
        {
            var response = await SendAsync(HttpMethod.Get, $"/v1/refunds?limit={limit}&page={page}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarListResponse<PolarRefund>>(json)
                   ?? throw new InvalidOperationException("Failed to deserialize refund list");
        }

        /// <summary>
        /// Gets a refund by id.
        /// </summary>
        /// <param name="refundId">Refund identifier.</param>
        /// <returns>The <see cref="PolarRefund"/> resource.</returns>
        /// <exception cref="HttpRequestException">Thrown when the HTTP request fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarRefund> GetRefundAsync(string refundId)
        {
            var response = await SendAsync(HttpMethod.Get, $"/v1/refunds/{refundId}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarRefund>(json)
                   ?? throw new InvalidOperationException("Failed to deserialize PolarRefund");
        }
    }
}