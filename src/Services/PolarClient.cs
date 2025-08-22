// Enable nullable reference types locally for this file
#nullable enable

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PolarNet.Models;

namespace PolarNet.Services
{
    /// <summary>
    /// A single lightweight client for the Polar REST API.
    /// </summary>
    /// <remarks>
    /// - Authentication: Uses a Bearer Organization Access Token (OAT) via the HTTP Authorization header.
    /// - Environment: Controlled by <see cref="PolarClientOptions.BaseUrl"/>.
    ///   Set it to the sandbox host (default: https://sandbox-api.polar.sh) or live host (https://api.polar.sh).
    /// - JSON: Uses System.Text.Json for serialization and deserialization.
    /// - Convenience defaults: Optional Organization/Product/Price IDs can be supplied in options and used by helper methods.
    /// </remarks>
    public sealed class PolarClient : IDisposable
    {
        private readonly HttpClient _http;
        private readonly string? _organizationId;
        private readonly string? _defaultProductId;
        private readonly string? _defaultPriceId;

        /// <summary>
        /// Initializes a new instance of <see cref="PolarClient"/> using explicit <see cref="PolarClientOptions"/>.
        /// </summary>
        /// <param name="options">
        /// Configuration for the client including the access token, base API URL, and optional default identifiers.
        /// - <see cref="PolarClientOptions.AccessToken"/> (required): Polar Organization Access Token.
        /// - <see cref="PolarClientOptions.BaseUrl"/> (optional): API base URL without trailing slash.
        ///   Defaults to https://sandbox-api.polar.sh.
        /// - <see cref="PolarClientOptions.OrganizationId"/>, <see cref="PolarClientOptions.DefaultProductId"/>, <see cref="PolarClientOptions.DefaultPriceId"/>: optional defaults.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <see cref="PolarClientOptions.AccessToken"/> is missing or whitespace.</exception>
        public PolarClient(PolarClientOptions options)
        {
            if (options is null) throw new ArgumentNullException(nameof(options));
            if (string.IsNullOrWhiteSpace(options.AccessToken))
                throw new ArgumentException("AccessToken is required", nameof(options));

            var baseUrl = options.BaseUrl.TrimEnd('/');

            _http = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.AccessToken);
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _organizationId = options.OrganizationId;
            _defaultProductId = options.DefaultProductId;
            _defaultPriceId = options.DefaultPriceId;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PolarClient"/> with a custom <see cref="HttpMessageHandler"/> (useful for testing).
        /// </summary>
        /// <param name="options">Client options (see <see cref="PolarClient(PolarClientOptions)"/>).</param>
        /// <param name="handler">Optional HTTP message handler used to create the <see cref="HttpClient"/>.</param>
        public PolarClient(PolarClientOptions options, HttpMessageHandler? handler)
        {
            if (options is null) throw new ArgumentNullException(nameof(options));
            if (string.IsNullOrWhiteSpace(options.AccessToken))
                throw new ArgumentException("AccessToken is required", nameof(options));

            var baseUrl = options.BaseUrl.TrimEnd('/');

            _http = handler is null ? new HttpClient() : new HttpClient(handler);
            _http.BaseAddress = new Uri(baseUrl);
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.AccessToken);
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _organizationId = options.OrganizationId;
            _defaultProductId = options.DefaultProductId;
            _defaultPriceId = options.DefaultPriceId;
        }

        // -------------------- Organization --------------------
        /// <summary>
        /// Gets the details of the configured organization.
        /// </summary>
        /// <returns>The <see cref="PolarOrganization"/> resource.</returns>
        /// <exception cref="ArgumentException">Thrown if <c>OrganizationId</c> is not configured in <see cref="PolarClientOptions"/>.</exception>
        /// <exception cref="HttpRequestException">Thrown if the HTTP request fails (via <see cref="HttpResponseMessage.EnsureSuccessStatusCode"/>).</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarOrganization> GetOrganizationAsync()
        {
            var orgId = _organizationId ?? throw new ArgumentException("OrganizationId must be provided in options");
            var response = await _http.GetAsync($"/v1/organizations/{orgId}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarOrganization>(json)
                   ?? throw new InvalidOperationException("Failed to deserialize PolarOrganization");
        }

        // -------------------- Products --------------------
        /// <summary>
        /// Gets a product by id. Falls back to <c>DefaultProductId</c> when <paramref name="productId"/> is not provided.
        /// </summary>
        /// <param name="productId">Optional product id. If null, <c>DefaultProductId</c> from options must be set.</param>
        /// <returns>The <see cref="PolarProduct"/> resource.</returns>
        /// <exception cref="ArgumentException">Thrown if neither <paramref name="productId"/> nor <c>DefaultProductId</c> is available.</exception>
        /// <exception cref="HttpRequestException">Thrown if the HTTP request fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarProduct> GetProductAsync(string? productId = null)
        {
            var pid = productId ?? _defaultProductId ?? throw new ArgumentException("ProductId must be supplied or DefaultProductId set in options");
            var response = await _http.GetAsync($"/v1/products/{pid}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarProduct>(json)
                   ?? throw new InvalidOperationException("Failed to deserialize PolarProduct");
        }

        /// <summary>
        /// Lists products filtered by the configured <c>OrganizationId</c>.
        /// </summary>
        /// <returns>Paged list containing products and pagination metadata.</returns>
        /// <exception cref="ArgumentException">Thrown if <c>OrganizationId</c> is not configured in options.</exception>
        /// <exception cref="HttpRequestException">Thrown if the HTTP request fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarListResponse<PolarProduct>> ListProductsAsync()
        {
            var orgId = _organizationId ?? throw new ArgumentException("OrganizationId must be provided in options");
            var response = await _http.GetAsync($"/v1/products?organization_id={orgId}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarListResponse<PolarProduct>>(json)
                   ?? throw new InvalidOperationException("Failed to deserialize product list");
        }

        // -------------------- Prices --------------------
        /// <summary>
        /// Gets a price by id.
        /// </summary>
        /// <param name="priceId">Price id.</param>
        /// <returns>The <see cref="PolarPrice"/> resource.</returns>
        /// <exception cref="HttpRequestException">Thrown if the HTTP request fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarPrice> GetPriceAsync(string priceId)
        {
            var response = await _http.GetAsync($"/v1/prices/{priceId}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarPrice>(json)
                   ?? throw new InvalidOperationException("Failed to deserialize PolarPrice");
        }

        /// <summary>
        /// Lists prices for the configured organization, optionally filtered by product.
        /// </summary>
        /// <param name="productId">Optional product id to filter prices for a specific product.</param>
        /// <returns>Paged list of prices.</returns>
        /// <exception cref="ArgumentException">Thrown if <c>OrganizationId</c> is not configured.</exception>
        /// <exception cref="HttpRequestException">Thrown if the HTTP request fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarListResponse<PolarPrice>> ListPricesAsync(string? productId = null)
        {
            var orgId = _organizationId ?? throw new ArgumentException("OrganizationId must be provided in options");
            var url = new StringBuilder($"/v1/prices?organization_id={orgId}");
            if (!string.IsNullOrWhiteSpace(productId))
            {
                url.Append($"&product_id={productId}");
            }

            var response = await _http.GetAsync(url.ToString());
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarListResponse<PolarPrice>>(json)
                   ?? throw new InvalidOperationException("Failed to deserialize price list");
        }

        // -------------------- Customers --------------------
        /// <summary>
        /// Creates a new customer in the configured organization.
        /// </summary>
        /// <param name="email">Customer email. Required by the API.</param>
        /// <param name="name">Optional display name. Defaults to the local part of the email.</param>
        /// <returns>The created <see cref="PolarCustomer"/>.</returns>
        /// <exception cref="ArgumentException">Thrown if <c>OrganizationId</c> is not configured.</exception>
        /// <exception cref="Exception">Thrown with response body when the API returns a non-success status code.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarCustomer> CreateCustomerAsync(string email, string? name = null)
        {
            var orgId = _organizationId ?? throw new ArgumentException("OrganizationId must be provided in options");
            var request = new CreateCustomerRequest
            {
                Email = email,
                Name = string.IsNullOrWhiteSpace(name) ? email.Split('@')[0] : name,
                OrganizationId = orgId
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync("/v1/customers", content);

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
        /// Gets a customer's aggregated state (active subscriptions, granted benefits, active meters).
        /// </summary>
        /// <param name="customerId">Customer id.</param>
        /// <returns>The <see cref="PolarCustomerState"/> object.</returns>
        /// <exception cref="HttpRequestException">Thrown if the HTTP request fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarCustomerState> GetCustomerStateAsync(string customerId)
        {
            var response = await _http.GetAsync($"/v1/customers/{customerId}/state");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarCustomerState>(json)
                   ?? throw new InvalidOperationException("Failed to deserialize PolarCustomerState");
        }

        /// <summary>
        /// Lists customers for the configured organization.
        /// </summary>
        /// <returns>Paged list of customers.</returns>
        /// <exception cref="ArgumentException">Thrown if <c>OrganizationId</c> is not configured.</exception>
        /// <exception cref="HttpRequestException">Thrown if the HTTP request fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarListResponse<PolarCustomer>> ListCustomersAsync()
        {
            var orgId = _organizationId ?? throw new ArgumentException("OrganizationId must be provided in options");
            var response = await _http.GetAsync($"/v1/customers?organization_id={orgId}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarListResponse<PolarCustomer>>(json)
                   ?? throw new InvalidOperationException("Failed to deserialize customer list");
        }

        /// <summary>
        /// Gets a customer by id.
        /// </summary>
        /// <param name="customerId">Customer id.</param>
        /// <returns>The <see cref="PolarCustomer"/> resource.</returns>
        /// <exception cref="HttpRequestException">Thrown if the HTTP request fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarCustomer> GetCustomerAsync(string customerId)
        {
            var response = await _http.GetAsync($"/v1/customers/{customerId}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarCustomer>(json)
                   ?? throw new InvalidOperationException("Failed to deserialize PolarCustomer");
        }

        // -------------------- Subscriptions --------------------
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
            var request = new CreateSubscriptionRequest { CustomerId = customerId, ProductPriceId = pid };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync("/v1/subscriptions", content);

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
        /// <returns>Paged list of subscriptions.</returns>
        /// <exception cref="ArgumentException">Thrown if <c>OrganizationId</c> is not configured.</exception>
        /// <exception cref="HttpRequestException">Thrown if the HTTP request fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarListResponse<PolarSubscription>> ListSubscriptionsAsync()
        {
            var orgId = _organizationId ?? throw new ArgumentException("OrganizationId must be provided in options");
            var response = await _http.GetAsync($"/v1/subscriptions?organization_id={orgId}");
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
            var response = await _http.GetAsync($"/v1/subscriptions/{subscriptionId}");
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
            var response = await _http.DeleteAsync($"/v1/subscriptions/{subscriptionId}");
            return response.IsSuccessStatusCode;
        }

        // -------------------- Checkouts --------------------
        /// <summary>
        /// Creates a custom checkout session using the default price configured in options.
        /// </summary>
        /// <param name="customerEmail">Optional customer email to prefill at checkout.</param>
        /// <param name="successUrl">Optional redirect URL after successful checkout. Defaults to a placeholder URL.</param>
        /// <returns>The created <see cref="PolarCheckout"/> session object.</returns>
        /// <exception cref="ArgumentException">Thrown if <c>DefaultPriceId</c> is not configured in options.</exception>
        /// <exception cref="Exception">Thrown with response body when the API returns a non-success status code.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarCheckout> CreateCheckoutAsync(string? customerEmail = null, string? successUrl = null)
        {
            var priceId = _defaultPriceId ?? throw new ArgumentException("DefaultPriceId must be provided in options");
            var request = new CreateCheckoutRequest
            {
                ProductPriceId = priceId,
                SuccessUrl = string.IsNullOrWhiteSpace(successUrl) ? "https://example.com/success" : successUrl,
                CustomerEmail = customerEmail ?? string.Empty,
                Metadata = new Dictionary<string, string>
                {
                    { "source", "polar-net" },
                    { "timestamp", DateTime.UtcNow.ToString("O") }
                }
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync("/v1/checkouts/custom", content);

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
            var response = await _http.GetAsync($"/v1/checkouts/custom/{checkoutId}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarCheckout>(json)
                   ?? throw new InvalidOperationException("Failed to deserialize PolarCheckout");
        }

        // -------------------- Orders & Benefits --------------------
        /// <summary>
        /// Lists orders for the configured organization.
        /// </summary>
        /// <returns>Paged list of orders.</returns>
        /// <exception cref="ArgumentException">Thrown if <c>OrganizationId</c> is not configured.</exception>
        /// <exception cref="HttpRequestException">Thrown if the HTTP request fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarListResponse<PolarOrder>> ListOrdersAsync()
        {
            var orgId = _organizationId ?? throw new ArgumentException("OrganizationId must be provided in options");
            var response = await _http.GetAsync($"/v1/orders?organization_id={orgId}");
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
            var response = await _http.GetAsync($"/v1/orders/{orderId}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarOrder>(json)
                   ?? throw new InvalidOperationException("Failed to deserialize PolarOrder");
        }

        /// <summary>
        /// Lists benefits for the configured organization.
        /// </summary>
        /// <returns>Paged list of benefits.</returns>
        /// <exception cref="ArgumentException">Thrown if <c>OrganizationId</c> is not configured.</exception>
        /// <exception cref="HttpRequestException">Thrown if the HTTP request fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarListResponse<PolarBenefit>> ListBenefitsAsync()
        {
            var orgId = _organizationId ?? throw new ArgumentException("OrganizationId must be provided in options");
            var response = await _http.GetAsync($"/v1/benefits?organization_id={orgId}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarListResponse<PolarBenefit>>(json)
                   ?? throw new InvalidOperationException("Failed to deserialize benefit list");
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _http.Dispose();
        }
    }
}