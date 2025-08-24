#if NETSTANDARD2_0 || NETSTANDARD2_1
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#else
#nullable enable
#endif

using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PolarNet.Models;

namespace PolarNet.Services
{
    public sealed partial class PolarClient
    {
        /// <summary>
        /// Creates a subscription for a customer using the provided price or the default price configured in options.
        /// </summary>
        /// <param name="customerId">Customer id.</param>
        /// <param name="priceId">Optional product price id. If not supplied, <c>DefaultPriceId</c> must be set.</param>
        /// <returns>The created <see cref="PolarSubscription"/>.</returns>
        /// <exception cref="ArgumentException">Thrown if neither <paramref name="priceId"/> nor <c>DefaultPriceId</c> is available.</exception>
        /// <exception cref="Exception">Thrown with response body when the API returns a non-success status code.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarSubscription> CreateSubscriptionAsync(string customerId, string? priceId = null)
        {
            var pid = priceId ?? _defaultPriceId ?? throw new ArgumentException("PriceId must be supplied or DefaultPriceId set in options");
            // Try POST /v1/customers/{customerId}/subscriptions with body { product_price_id }
            var request = new CreateSubscriptionRequest { ProductPriceId = pid };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await SendAsync(HttpMethod.Post, $"/v1/customers/{customerId}/subscriptions", content);

            // If not found or method not allowed, fallback to POST /v1/subscriptions with { customer_id, product_price_id }
            if (!response.IsSuccessStatusCode &&
                (response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.MethodNotAllowed))
            {
                var altPayload = new
                {
                    customer_id = customerId,
                    product_price_id = pid
                };
                var altJson = JsonSerializer.Serialize(altPayload);
                var altContent = new StringContent(altJson, Encoding.UTF8, "application/json");
                using var altResponse = await SendAsync(HttpMethod.Post, "/v1/subscriptions", altContent);

                if (!altResponse.IsSuccessStatusCode)
                {
                    var altError = await altResponse.Content.ReadAsStringAsync();
                    throw new Exception($"Failed to create subscription: {altResponse.StatusCode} - {altError}");
                }

                var altResponseContent = await altResponse.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<PolarSubscription>(altResponseContent)
                       ?? throw new InvalidOperationException("Failed to deserialize PolarSubscription");
            }

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to create subscription: {response.StatusCode} - {error}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarSubscription>(responseContent)
                   ?? throw new InvalidOperationException("Failed to deserialize PolarSubscription");
        }

        /// <summary>
        /// Lists subscriptions for the configured organization.
        /// </summary>
        /// <param name="page">1-based page number. Defaults to 1.</param>
        /// <param name="limit">Items per page (1-100). Defaults to 10.</param>
        /// <returns>Paged list of subscriptions.</returns>
        /// <exception cref="ArgumentException">Thrown if <c>OrganizationId</c> is not configured.</exception>
        /// <exception cref="HttpRequestException">Thrown if the HTTP request fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarListResponse<PolarSubscription>> ListSubscriptionsAsync(int page = 1, int limit = 10)
        {
            using var response = await SendAsync(HttpMethod.Get, $"/v1/subscriptions?limit={limit}&page={page}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarListResponse<PolarSubscription>>(json)
                   ?? throw new InvalidOperationException("Failed to deserialize subscription list");
        }

        /// <summary>
        /// Gets a subscription by id.
        /// </summary>
        /// <param name="subscriptionId">Subscription id.</param>
        /// <returns>The <see cref="PolarSubscription"/> resource.</returns>
        /// <exception cref="HttpRequestException">Thrown if the HTTP request fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarSubscription> GetSubscriptionAsync(string subscriptionId)
        {
            using var response = await SendAsync(HttpMethod.Get, $"/v1/subscriptions/{subscriptionId}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarSubscription>(json)
                   ?? throw new InvalidOperationException("Failed to deserialize PolarSubscription");
        }

        /// <summary>
        /// Cancels a subscription by id.
        /// </summary>
        /// <param name="subscriptionId">Subscription id.</param>
        /// <returns><c>true</c> when the API responds with a success (2xx) status code; otherwise <c>false</c>.</returns>
        public async Task<bool> CancelSubscriptionAsync(string subscriptionId)
        {
            using var response = await SendAsync(HttpMethod.Delete, $"/v1/subscriptions/{subscriptionId}");
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Revokes a subscription immediately, removing access/benefits.
        /// </summary>
        /// <param name="subscriptionId">Subscription id.</param>
        /// <returns><c>true</c> when the API responds with a success (2xx) status code; otherwise <c>false</c>.</returns>
        /// <remarks>API doc: https://docs.polar.sh/api-reference/subscriptions/revoke</remarks>
        public async Task<bool> RevokeSubscriptionAsync(string subscriptionId)
        {
            // Primary path: POST /v1/subscriptions/{id}/revoke with empty body
            var content = new StringContent("{}", Encoding.UTF8, "application/json");
            using var response = await SendAsync(HttpMethod.Post, $"/v1/subscriptions/{subscriptionId}/revoke", content);

            // Fallback path when not found/method not allowed: POST /v1/subscriptions/revoke { subscription_id }
            if (!response.IsSuccessStatusCode &&
                (response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.MethodNotAllowed))
            {
                var payload = new { subscription_id = subscriptionId };
                var json = JsonSerializer.Serialize(payload);
                var altContent = new StringContent(json, Encoding.UTF8, "application/json");
                using var altResponse = await SendAsync(HttpMethod.Post, "/v1/subscriptions/revoke", altContent);
                return altResponse.IsSuccessStatusCode;
            }

            return response.IsSuccessStatusCode;
        }
    }
}