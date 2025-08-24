#if NETSTANDARD2_0 || NETSTANDARD2_1
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#else
#nullable enable
#endif

using System.Threading.Tasks;
using PolarNet.Models;
using System.Text.Json;
using System.Net.Http;
using System;

namespace PolarNet.Services
{
    /// <summary>
    /// Customer-related endpoints.
    /// </summary>
    public sealed partial class PolarClient
    {
        /// <summary>
        /// Creates a customer with the provided email and optional name.
        /// </summary>
        /// <param name="email">Customer email (required).</param>
        /// <param name="name">Optional name; falls back to the local-part of the email if not provided.</param>
        /// <returns>The created <see cref="PolarCustomer"/>.</returns>
        /// <exception cref="Exception">Thrown with response details when the API returns a non-success status code.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarCustomer> CreateCustomerAsync(string email, string? name = null)
        {
            var request = new CreateCustomerRequest
            {
                Email = email,
                Name = string.IsNullOrWhiteSpace(name) ? (email?.Split('@')[0] ?? "") : name!,
                OrganizationId = null
            };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            using var response = await SendAsync(HttpMethod.Post, "/v1/customers", content);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to create customer: {response.StatusCode} - {error}");
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarCustomer>(responseContent)
                   ?? throw new InvalidOperationException("Failed to deserialize PolarCustomer");
        }

        /// <summary>
        /// Creates a customer using a fully-specified request payload.
        /// </summary>
        /// <param name="request">Customer creation payload.</param>
        /// <returns>The created <see cref="PolarCustomer"/>.</returns>
        /// <exception cref="Exception">Thrown with response details when the API returns a non-success status code.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarCustomer> CreateCustomerAsync(CreateCustomerRequest request)
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            using var response = await SendAsync(HttpMethod.Post, "/v1/customers", content);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to create customer: {response.StatusCode} - {error}");
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarCustomer>(responseContent)
                   ?? throw new InvalidOperationException("Failed to deserialize PolarCustomer");
        }

        /// <summary>
        /// Retrieves the computed state for a customer (subscriptions, benefits, meters).
        /// </summary>
        /// <param name="customerId">Customer identifier.</param>
        /// <returns>The <see cref="PolarCustomerState"/>.</returns>
        /// <exception cref="HttpRequestException">Thrown when the HTTP request fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarCustomerState> GetCustomerStateAsync(string customerId)
        {
            using var response = await SendAsync(HttpMethod.Get, $"/v1/customers/{customerId}/state");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarCustomerState>(json)
                   ?? throw new InvalidOperationException("Failed to deserialize PolarCustomerState");
        }

        /// <summary>
        /// Lists customers with pagination.
        /// </summary>
        /// <param name="page">1-based page number. Defaults to 1.</param>
        /// <param name="limit">Items per page (1-100). Defaults to 10.</param>
        /// <returns>Paged list of customers.</returns>
        /// <exception cref="HttpRequestException">Thrown when the HTTP request fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarListResponse<PolarCustomer>> ListCustomersAsync(int page = 1, int limit = 10)
        {
            using var response = await SendAsync(HttpMethod.Get, $"/v1/customers?limit={limit}&page={page}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarListResponse<PolarCustomer>>(json)
                   ?? throw new InvalidOperationException("Failed to deserialize customer list");
        }

        /// <summary>
        /// Retrieves a customer by id.
        /// </summary>
        /// <param name="customerId">Customer identifier.</param>
        /// <returns>The <see cref="PolarCustomer"/>.</returns>
        /// <exception cref="HttpRequestException">Thrown when the HTTP request fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarCustomer> GetCustomerAsync(string customerId)
        {
            using var response = await SendAsync(HttpMethod.Get, $"/v1/customers/{customerId}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarCustomer>(json)
                   ?? throw new InvalidOperationException("Failed to deserialize PolarCustomer");
        }

        /// <summary>
        /// Deletes a customer by id.
        /// </summary>
        /// <param name="customerId">Customer identifier.</param>
        /// <returns><c>true</c> when the API responds with success (2xx); otherwise <c>false</c>.</returns>
        public async Task<bool> DeleteCustomerAsync(string customerId)
        {
            using var response = await SendAsync(HttpMethod.Delete, $"/v1/customers/{customerId}");
            return response.IsSuccessStatusCode;
        }
    }
}
