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
using System.Net;

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
            : this(options, (HttpMessageHandler?)null)
        {
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

            // Always disable auto-redirect to avoid losing Authorization on redirects
            if (handler is null)
            {
                handler = new HttpClientHandler { AllowAutoRedirect = false };
            }
            else if (handler is HttpClientHandler h)
            {
                h.AllowAutoRedirect = false;
            }

            _http = new HttpClient(handler);
            _http.BaseAddress = new Uri(baseUrl);
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.AccessToken);
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _organizationId = options.OrganizationId;
            _defaultProductId = options.DefaultProductId;
            _defaultPriceId = options.DefaultPriceId;
        }

        /// <summary>
        /// Sends an HTTP request and manually follows redirects while preserving Authorization headers.
        /// </summary>
        /// <param name="method">The HTTP method to use (GET, POST, etc.).</param>
        /// <param name="url">The request URL. Can be relative to the configured BaseAddress or an absolute URL.</param>
        /// <param name="content">Optional request body content. Typically null for GET/HEAD.</param>
        /// <returns>The final <see cref="HttpResponseMessage"/> after following redirects (up to 5).</returns>
        /// <remarks>
        /// This method preserves the Authorization header across redirects and recreates <see cref="StringContent"/>
        /// for non-idempotent methods when necessary. It does not throw for non-success status codes; callers should
        /// check <see cref="HttpResponseMessage.IsSuccessStatusCode"/> or call <see cref="HttpResponseMessage.EnsureSuccessStatusCode"/>.
        /// </remarks>
        private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, HttpContent? content = null)
        {
            const int maxRedirects = 5;
            int redirects = 0;
            string currentUrl = url;

            while (true)
            {
                using var req = new HttpRequestMessage(method, currentUrl) { Content = content };
                var resp = await _http.SendAsync(req);

                if (resp.StatusCode is HttpStatusCode.MovedPermanently // 301
                    or HttpStatusCode.Found                          // 302
                    or HttpStatusCode.SeeOther                       // 303
                    or HttpStatusCode.TemporaryRedirect              // 307
                    || (int)resp.StatusCode == 308)                  // 308 Permanent Redirect
                {
                    if (redirects++ >= maxRedirects)
                    {
                        return resp; // give up; let caller handle
                    }

                    if (resp.Headers.Location is null)
                    {
                        return resp; // no location to follow
                    }

                    // Prepare for next loop; clone content for non-GET methods
                    currentUrl = resp.Headers.Location.IsAbsoluteUri
                        ? resp.Headers.Location.ToString()
                        : new Uri(_http.BaseAddress!, resp.Headers.Location).ToString();

                    // For POST/PUT/PATCH with StringContent, recreate content to allow resend
                    if (content is StringContent sc && method != HttpMethod.Get && method != HttpMethod.Head)
                    {
                        var payload = await sc.ReadAsStringAsync();
                        content = new StringContent(payload, Encoding.UTF8, sc.Headers.ContentType?.MediaType ?? "application/json");
                    }

                    // Dispose previous response and continue
                    resp.Dispose();
                    continue;
                }

                return resp;
            }
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
            var response = await SendAsync(HttpMethod.Get, $"/v1/organizations/{orgId}");
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
            var response = await SendAsync(HttpMethod.Get, $"/v1/products/{pid}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarProduct>(json)
                   ?? throw new InvalidOperationException("Failed to deserialize PolarProduct");
        }

        /// <summary>
        /// Lists products filtered by the configured <c>OrganizationId</c>.
        /// </summary>
        /// <param name="page">1-based page number. Defaults to 1.</param>
        /// <param name="limit">Items per page (1-100). Defaults to 10.</param>
        /// <returns>Paged list containing products and pagination metadata.</returns>
        /// <exception cref="ArgumentException">Thrown if <c>OrganizationId</c> is not configured in options.</exception>
        /// <exception cref="HttpRequestException">Thrown if the HTTP request fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarListResponse<PolarProduct>> ListProductsAsync(int page = 1, int limit = 10)
        {
            // Rely on token's organization context; include sensible default pagination.
            var response = await SendAsync(HttpMethod.Get, $"/v1/products?limit={limit}&page={page}");
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
            var response = await SendAsync(HttpMethod.Get, $"/v1/prices/{priceId}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarPrice>(json)
                   ?? throw new InvalidOperationException("Failed to deserialize PolarPrice");
        }

        /// <summary>
        /// Lists prices for a product.
        /// </summary>
        /// <param name="productId">Product id to list prices for.</param>
        /// <param name="page">1-based page number. Defaults to 1.</param>
        /// <param name="limit">Items per page (1-100). Defaults to 10.</param>
        /// <returns>Paged list of prices.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="productId"/> is null or whitespace.</exception>
        /// <exception cref="HttpRequestException">Thrown if the HTTP request fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarListResponse<PolarPrice>> ListPricesAsync(string productId, int page = 1, int limit = 10)
        {
            if (string.IsNullOrWhiteSpace(productId)) throw new ArgumentException("productId is required", nameof(productId));
            var response = await SendAsync(HttpMethod.Get, $"/v1/products/{productId}/prices?limit={limit}&page={page}");
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
        /// <remarks>API doc: https://docs.polar.sh/api-reference/customers/create</remarks>
        public async Task<PolarCustomer> CreateCustomerAsync(string email, string? name = null)
        {
            var request = new CreateCustomerRequest
            {
                Email = email,
                Name = string.IsNullOrWhiteSpace(name) ? (email?.Split('@')[0] ?? "") : name!,
                // Do not send organization_id when using an organization token.
                OrganizationId = null
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await SendAsync(HttpMethod.Post, "/v1/customers", content);

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
        /// Advanced overload to create a customer with full payload (metadata, external_id, billing_address, tax_id, etc.).
        /// </summary>
        /// <param name="request">A fully-populated create request. When using an org token, leave OrganizationId null.</param>
        /// <returns>The created <see cref="PolarCustomer"/>.</returns>
        /// <remarks>API doc: https://docs.polar.sh/api-reference/customers/create</remarks>
        public async Task<PolarCustomer> CreateCustomerAsync(CreateCustomerRequest request)
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await SendAsync(HttpMethod.Post, "/v1/customers", content);

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
            var response = await SendAsync(HttpMethod.Get, $"/v1/customers/{customerId}/state");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarCustomerState>(json)
                   ?? throw new InvalidOperationException("Failed to deserialize PolarCustomerState");
        }

        /// <summary>
        /// Lists customers for the configured organization.
        /// </summary>
        /// <param name="page">1-based page number. Defaults to 1.</param>
        /// <param name="limit">Items per page (1-100). Defaults to 10.</param>
        /// <returns>Paged list of customers.</returns>
        /// <exception cref="ArgumentException">Thrown if <c>OrganizationId</c> is not configured.</exception>
        /// <exception cref="HttpRequestException">Thrown if the HTTP request fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarListResponse<PolarCustomer>> ListCustomersAsync(int page = 1, int limit = 10)
        {
            var response = await SendAsync(HttpMethod.Get, $"/v1/customers?limit={limit}&page={page}");
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
            var response = await SendAsync(HttpMethod.Get, $"/v1/customers/{customerId}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarCustomer>(json)
                   ?? throw new InvalidOperationException("Failed to deserialize PolarCustomer");
        }

        /// <summary>
        /// Deletes a customer by id.
        /// </summary>
        /// <param name="customerId">Customer id.</param>
        /// <returns><c>true</c> when the API responds with a success (2xx) status code; otherwise <c>false</c>.</returns>
        /// <remarks>API doc: https://docs.polar.sh/api-reference/customers/delete</remarks>
        public async Task<bool> DeleteCustomerAsync(string customerId)
        {
            var response = await SendAsync(HttpMethod.Delete, $"/v1/customers/{customerId}");
            return response.IsSuccessStatusCode;
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
            // Try POST /v1/customers/{customerId}/subscriptions with body { product_price_id }
            var request = new CreateSubscriptionRequest { ProductPriceId = pid };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await SendAsync(HttpMethod.Post, $"/v1/customers/{customerId}/subscriptions", content);

            // If not found or method not allowed, fallback to POST /v1/subscriptions with { customer_id, product_price_id }
            if (!response.IsSuccessStatusCode &&
                (response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.MethodNotAllowed))
            {
                response.Dispose();
                var altPayload = new
                {
                    customer_id = customerId,
                    product_price_id = pid
                };
                var altJson = JsonSerializer.Serialize(altPayload);
                var altContent = new StringContent(altJson, Encoding.UTF8, "application/json");
                response = await SendAsync(HttpMethod.Post, "/v1/subscriptions", altContent);
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
            var response = await SendAsync(HttpMethod.Get, $"/v1/subscriptions?limit={limit}&page={page}");
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
            var response = await SendAsync(HttpMethod.Get, $"/v1/subscriptions/{subscriptionId}");
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
            var response = await SendAsync(HttpMethod.Delete, $"/v1/subscriptions/{subscriptionId}");
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
            var response = await SendAsync(HttpMethod.Post, $"/v1/subscriptions/{subscriptionId}/revoke", content);

            // Fallback path when not found/method not allowed: POST /v1/subscriptions/revoke { subscription_id }
            if (!response.IsSuccessStatusCode &&
                (response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.MethodNotAllowed))
            {
                response.Dispose();
                var payload = new { subscription_id = subscriptionId };
                var json = JsonSerializer.Serialize(payload);
                var altContent = new StringContent(json, Encoding.UTF8, "application/json");
                response = await SendAsync(HttpMethod.Post, "/v1/subscriptions/revoke", altContent);
            }

            return response.IsSuccessStatusCode;
        }

        // -------------------- Checkouts --------------------
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
                CustomerEmail = customerEmail ?? string.Empty,
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

        // -------------------- Orders & Benefits --------------------
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
            var response = await SendAsync(HttpMethod.Get, $"/v1/orders?limit={limit}&page={page}");
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
            var response = await SendAsync(HttpMethod.Get, $"/v1/orders/{orderId}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarOrder>(json)
                   ?? throw new InvalidOperationException("Failed to deserialize PolarOrder");
        }

        /// <summary>
        /// Lists benefits for the configured organization.
        /// </summary>
        /// <param name="page">1-based page number. Defaults to 1.</param>
        /// <param name="limit">Items per page (1-100). Defaults to 10.</param>
        /// <returns>Paged list of benefits.</returns>
        /// <exception cref="ArgumentException">Thrown if <c>OrganizationId</c> is not configured.</exception>
        /// <exception cref="HttpRequestException">Thrown if the HTTP request fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarListResponse<PolarBenefit>> ListBenefitsAsync(int page = 1, int limit = 10)
        {
            var response = await SendAsync(HttpMethod.Get, $"/v1/benefits?limit={limit}&page={page}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarListResponse<PolarBenefit>>(json)
                   ?? throw new InvalidOperationException("Failed to deserialize benefit list");
        }

        /// <summary>
        /// Disposes the client and releases the underlying HTTP resources.
        /// </summary>
        /// <remarks>
        /// After calling this method, the instance should no longer be used to perform API calls.
        /// </remarks>
        public void Dispose()
        {
            _http.Dispose();
        }
    }
}