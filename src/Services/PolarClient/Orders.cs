using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using PolarNet.Models;
using PolarNet.Models.Resources;

namespace PolarNet.Services
{
    public sealed partial class PolarClient
    {
        /// <summary>
        /// Lists orders for the configured organization.
        /// </summary>
        /// <param name="page">1-based page number. Defaults to 1.</param>
        /// <param name="limit">Items per page (1-100). Defaults to 10.</param>
        /// <returns>Paged list of orders.</returns>
        /// <exception cref="ArgumentException">Thrown if <c>OrganizationId</c> is not configured.</exception>
        /// <exception cref="HttpRequestException">Thrown if the HTTP request fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarListResponse<PolarOrder>> ListOrdersAsync(int page = 1, int limit = 10)
        {
            using var response = await SendAsync(HttpMethod.Get, $"/v1/orders?limit={limit}&page={page}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarListResponse<PolarOrder>>(json)
                   ?? throw new InvalidOperationException("Failed to deserialize order list");
        }

        /// <summary>
        /// Gets an order by id.
        /// </summary>
        /// <param name="orderId">Order id.</param>
        /// <returns>The <see cref="PolarOrder"/> resource.</returns>
        /// <exception cref="HttpRequestException">Thrown if the HTTP request fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarOrder> GetOrderAsync(string orderId)
        {
            using var response = await SendAsync(HttpMethod.Get, $"/v1/orders/{orderId}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarOrder>(json)
                   ?? throw new InvalidOperationException("Failed to deserialize PolarOrder");
        }

        /// <summary>
        /// Requests invoice generation for an order.
        /// </summary>
        /// <remarks>
        /// API: POST /v1/orders/{id}/invoice
        /// </remarks>
        public async Task<bool> GenerateOrderInvoiceAsync(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId)) throw new ArgumentException("orderId is required", nameof(orderId));
            using var response = await SendAsync(HttpMethod.Post, $"/v1/orders/{orderId}/invoice", new StringContent("{}", System.Text.Encoding.UTF8, "application/json"));
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Retrieves an invoice for an order. Ensure invoice has been generated first.
        /// </summary>
        /// <remarks>
        /// API: GET /v1/orders/{id}/invoice
        /// </remarks>
        public async Task<PolarInvoice> GetOrderInvoiceAsync(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId)) throw new ArgumentException("orderId is required", nameof(orderId));
            using var response = await SendAsync(HttpMethod.Get, $"/v1/orders/{orderId}/invoice");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarInvoice>(json) ?? throw new InvalidOperationException("Failed to deserialize PolarInvoice");
        }
    }
}