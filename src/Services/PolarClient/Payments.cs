#if NETSTANDARD2_0 || NETSTANDARD2_1
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#else
#nullable enable
#endif

using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using PolarNet.Models;
using PolarNet.Models.Resources;

namespace PolarNet.Services
{
    /// <summary>
    /// Payment-related endpoints.
    /// </summary>
    public sealed partial class PolarClient
    {
        /// <summary>
        /// Lists payments for the configured organization.
        /// </summary>
        /// <param name="page">1-based page number. Defaults to 1.</param>
        /// <param name="limit">Items per page (1-100). Defaults to 10.</param>
        /// <returns>Paged list of payments.</returns>
        /// <exception cref="HttpRequestException">Thrown when the HTTP request fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarListResponse<PolarPayment>> ListPaymentsAsync(int page = 1, int limit = 10)
        {
            using var response = await SendAsync(HttpMethod.Get, $"/v1/payments?limit={limit}&page={page}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarListResponse<PolarPayment>>(json)
                   ?? throw new InvalidOperationException("Failed to deserialize payment list");
        }

        /// <summary>
        /// Gets a payment by id.
        /// </summary>
        /// <param name="paymentId">Payment identifier.</param>
        /// <returns>The <see cref="PolarPayment"/> resource.</returns>
        /// <exception cref="HttpRequestException">Thrown when the HTTP request fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarPayment> GetPaymentAsync(string paymentId)
        {
            using var response = await SendAsync(HttpMethod.Get, $"/v1/payments/{paymentId}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarPayment>(json)
                   ?? throw new InvalidOperationException("Failed to deserialize PolarPayment");
        }

        /// <summary>
        /// Lists payments for a specific order.
        /// </summary>
        /// <param name="orderId">Order identifier to filter payments.</param>
        /// <param name="page">1-based page number. Defaults to 1.</param>
        /// <param name="limit">Items per page (1-100). Defaults to 10.</param>
        /// <returns>Paged list of payments for the order.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="orderId"/> is empty.</exception>
        /// <exception cref="HttpRequestException">Thrown when the HTTP request fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarListResponse<PolarPayment>> ListPaymentsByOrderAsync(string orderId, int page = 1, int limit = 10)
        {
            if (string.IsNullOrWhiteSpace(orderId)) 
                throw new ArgumentException("orderId is required", nameof(orderId));
            
            using var response = await SendAsync(HttpMethod.Get, $"/v1/payments?order_id={orderId}&limit={limit}&page={page}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarListResponse<PolarPayment>>(json)
                   ?? throw new InvalidOperationException("Failed to deserialize payment list");
        }

        /// <summary>
        /// Lists payments for a specific customer.
        /// </summary>
        /// <param name="customerId">Customer identifier to filter payments.</param>
        /// <param name="page">1-based page number. Defaults to 1.</param>
        /// <param name="limit">Items per page (1-100). Defaults to 10.</param>
        /// <returns>Paged list of payments for the customer.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="customerId"/> is empty.</exception>
        /// <exception cref="HttpRequestException">Thrown when the HTTP request fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarListResponse<PolarPayment>> ListPaymentsByCustomerAsync(string customerId, int page = 1, int limit = 10)
        {
            if (string.IsNullOrWhiteSpace(customerId)) 
                throw new ArgumentException("customerId is required", nameof(customerId));
            
            using var response = await SendAsync(HttpMethod.Get, $"/v1/payments?customer_id={customerId}&limit={limit}&page={page}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarListResponse<PolarPayment>>(json)
                   ?? throw new InvalidOperationException("Failed to deserialize payment list");
        }
    }
}