#if NETSTANDARD2_0 || NETSTANDARD2_1
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#else
#nullable enable
#endif

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PolarNet.Models;
using PolarNet.Models.Resources;

namespace PolarNet.Services
{
    /// <summary>
    /// Webhook endpoint management.
    /// </summary>
    public sealed partial class PolarClient
    {
        /// <summary>
        /// Creates a new webhook endpoint.
        /// </summary>
        /// <param name="request">Webhook endpoint creation request.</param>
        /// <returns>The created <see cref="PolarWebhookEndpoint"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
        /// <exception cref="Exception">Thrown with response details when the API returns a non-success status code.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarWebhookEndpoint> CreateWebhookEndpointAsync(CreateWebhookEndpointRequest request)
        {
            if (request is null) 
                throw new ArgumentNullException(nameof(request));

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await SendAsync(HttpMethod.Post, "/v1/webhooks/endpoints", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to create webhook endpoint: {response.StatusCode} - {error}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarWebhookEndpoint>(responseContent)
                   ?? throw new InvalidOperationException("Failed to deserialize PolarWebhookEndpoint");
        }

        /// <summary>
        /// Creates a new webhook endpoint with simplified parameters.
        /// </summary>
        /// <param name="url">The URL to receive webhook events.</param>
        /// <param name="events">List of event types to subscribe to (e.g., "order.created", "subscription.created").</param>
        /// <param name="secret">Optional secret for webhook signature verification.</param>
        /// <returns>The created <see cref="PolarWebhookEndpoint"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="url"/> is empty or <paramref name="events"/> is null/empty.</exception>
        /// <exception cref="Exception">Thrown with response details when the API returns a non-success status code.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarWebhookEndpoint> CreateWebhookEndpointAsync(string url, List<string> events, string? secret = null)
        {
            if (string.IsNullOrWhiteSpace(url)) 
                throw new ArgumentException("url is required", nameof(url));
            if (events is null || events.Count == 0) 
                throw new ArgumentException("events list cannot be empty", nameof(events));

            var request = new CreateWebhookEndpointRequest
            {
                Url = url,
                Events = events,
                Secret = secret,
                Format = "raw"
            };

            return await CreateWebhookEndpointAsync(request);
        }

        /// <summary>
        /// Lists webhook endpoints for the configured organization.
        /// </summary>
        /// <param name="page">1-based page number. Defaults to 1.</param>
        /// <param name="limit">Items per page (1-100). Defaults to 10.</param>
        /// <returns>Paged list of webhook endpoints.</returns>
        /// <exception cref="HttpRequestException">Thrown when the HTTP request fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarListResponse<PolarWebhookEndpoint>> ListWebhookEndpointsAsync(int page = 1, int limit = 10)
        {
            using var response = await SendAsync(HttpMethod.Get, $"/v1/webhooks/endpoints?limit={limit}&page={page}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarListResponse<PolarWebhookEndpoint>>(json)
                   ?? throw new InvalidOperationException("Failed to deserialize webhook endpoint list");
        }

        /// <summary>
        /// Gets a webhook endpoint by id.
        /// </summary>
        /// <param name="endpointId">Webhook endpoint identifier.</param>
        /// <returns>The <see cref="PolarWebhookEndpoint"/> resource.</returns>
        /// <exception cref="HttpRequestException">Thrown when the HTTP request fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarWebhookEndpoint> GetWebhookEndpointAsync(string endpointId)
        {
            using var response = await SendAsync(HttpMethod.Get, $"/v1/webhooks/endpoints/{endpointId}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarWebhookEndpoint>(json)
                   ?? throw new InvalidOperationException("Failed to deserialize PolarWebhookEndpoint");
        }

        /// <summary>
        /// Updates a webhook endpoint.
        /// </summary>
        /// <param name="endpointId">Webhook endpoint identifier.</param>
        /// <param name="request">Update request with fields to modify.</param>
        /// <returns>The updated <see cref="PolarWebhookEndpoint"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
        /// <exception cref="Exception">Thrown with response details when the API returns a non-success status code.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarWebhookEndpoint> UpdateWebhookEndpointAsync(string endpointId, UpdateWebhookEndpointRequest request)
        {
            if (request is null) 
                throw new ArgumentNullException(nameof(request));

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await SendAsync(new HttpMethod("PATCH"), $"/v1/webhooks/endpoints/{endpointId}", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to update webhook endpoint: {response.StatusCode} - {error}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarWebhookEndpoint>(responseContent)
                   ?? throw new InvalidOperationException("Failed to deserialize PolarWebhookEndpoint");
        }

        /// <summary>
        /// Deletes a webhook endpoint.
        /// </summary>
        /// <param name="endpointId">Webhook endpoint identifier.</param>
        /// <returns><c>true</c> when the API responds with success (2xx); otherwise <c>false</c>.</returns>
        public async Task<bool> DeleteWebhookEndpointAsync(string endpointId)
        {
            using var response = await SendAsync(HttpMethod.Delete, $"/v1/webhooks/endpoints/{endpointId}");
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Tests a webhook endpoint by sending a test event.
        /// </summary>
        /// <param name="endpointId">Webhook endpoint identifier.</param>
        /// <param name="eventType">Optional event type to test (defaults to "ping").</param>
        /// <returns><c>true</c> when the test succeeds; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// Note: The test may return false if the webhook URL is not reachable.
        /// This is expected behavior for test endpoints that don't actually exist.
        /// </remarks>
        public async Task<bool> TestWebhookEndpointAsync(string endpointId, string? eventType = null)
        {
            try
            {
                var url = string.IsNullOrWhiteSpace(eventType) 
                    ? $"/v1/webhooks/endpoints/{endpointId}/test"
                    : $"/v1/webhooks/endpoints/{endpointId}/test?event={eventType}";
                
                using var response = await SendAsync(HttpMethod.Post, url, new StringContent("{}", Encoding.UTF8, "application/json"));

                // The API typically returns 200 OK even if the webhook delivery fails
                // Check the response content for actual success
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    // Empty or success response means the test was initiated successfully
                    // The actual webhook delivery may still fail if the URL is unreachable
                    return string.IsNullOrWhiteSpace(content) || !content.Contains("\"error\"");
                }

                return false;
            }
            catch
            {
                // If there's an exception, the test failed
                return false;
            }
        }
    }
}