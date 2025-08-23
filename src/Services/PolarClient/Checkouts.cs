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

namespace PolarNet.Services
{
    public sealed partial class PolarClient
    {
        /// <summary>
        /// Creates a custom checkout session using the default price configured in options.
        /// </summary>
        /// <param name="customerEmail">Optional customer email to prefill at checkout.</param>
        /// <param name="successUrl">Required redirect URL after successful checkout.</param>
        /// <returns>The created <see cref="PolarCheckout"/> session object.</returns>
        /// <exception cref="ArgumentException">Thrown if <c>DefaultPriceId</c> is not configured in options, or when <paramref name="successUrl"/> is null/whitespace.</exception>
        /// <exception cref="Exception">Thrown with response body when the API returns a non-success status code.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarCheckout> CreateCheckoutAsync(string? customerEmail = null, string? successUrl = null)
        {
            var priceId = _defaultPriceId ?? throw new ArgumentException("DefaultPriceId must be provided in options");
            if (string.IsNullOrWhiteSpace(successUrl))
                throw new ArgumentException("successUrl is required", nameof(successUrl));

            var request = new CreateCheckoutRequest
            {
                ProductPriceId = priceId,
                SuccessUrl = successUrl,
                CustomerEmail = customerEmail,  // Let it be null if not provided
                Metadata = new Dictionary<string, string>
                {
                    { "source", "polar-net" },
                    { "timestamp", DateTime.UtcNow.ToString("O") }
                }
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await SendAsync(HttpMethod.Post, "/v1/checkouts", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to create checkout: {response.StatusCode} - {error}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarCheckout>(responseContent)
                   ?? throw new InvalidOperationException("Failed to deserialize PolarCheckout");
        }

        /// <summary>
        /// Gets a custom checkout session by id.
        /// </summary>
        /// <param name="checkoutId">Checkout session id.</param>
        /// <returns>The <see cref="PolarCheckout"/> resource.</returns>
        /// <exception cref="HttpRequestException">Thrown if the HTTP request fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarCheckout> GetCheckoutAsync(string checkoutId)
        {
            var response = await SendAsync(HttpMethod.Get, $"/v1/checkouts/{checkoutId}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarCheckout>(json)
                   ?? throw new InvalidOperationException("Failed to deserialize PolarCheckout");
        }
    }
}