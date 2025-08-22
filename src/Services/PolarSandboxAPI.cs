// Enable nullable reference types locally for this file
#nullable enable

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using PolarNet.Models;
using Microsoft.Extensions.Configuration;

namespace PolarNet.Services
{
    public class PolarSandboxAPI : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _organizationId;
        private readonly string _productId;
        private readonly string _priceId;

        public PolarSandboxAPI(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();

            var useSandbox = _configuration["PolarSettings:UseSandbox"] == "true";
            var baseUrl = useSandbox
                ? _configuration["PolarSettings:SandboxApiUrl"]
                : _configuration["PolarSettings:ProductionApiUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new ArgumentException("PolarSettings:SandboxApiUrl or ProductionApiUrl must be configured");
            }
            _httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _configuration["PolarSettings:AccessToken"]);
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            _organizationId = _configuration["PolarSettings:OrganizationId"] ?? throw new ArgumentException("PolarSettings:OrganizationId is required");
            _productId = _configuration["PolarSettings:ProductId"] ?? throw new ArgumentException("PolarSettings:ProductId is required");
            _priceId = _configuration["PolarSettings:PriceId"] ?? throw new ArgumentException("PolarSettings:PriceId is required");
        }

        // Organization endpoints
        public async Task<PolarOrganization> GetOrganizationAsync()
        {
            var response = await _httpClient.GetAsync($"/v1/organizations/{_organizationId}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<PolarOrganization>(json)
                ?? throw new InvalidOperationException("Failed to deserialize PolarOrganization");
        }

        // Product endpoints
        public async Task<PolarProduct> GetProductAsync(string? productId = null)
        {
            productId ??= _productId;
            var response = await _httpClient.GetAsync($"/v1/products/{productId}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<PolarProduct>(json)
                   ?? throw new InvalidOperationException("Failed to deserialize PolarProduct");
        }

        public async Task<PolarListResponse<PolarProduct>> ListProductsAsync()
        {
            var response = await _httpClient.GetAsync($"/v1/products?organization_id={_organizationId}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<PolarListResponse<PolarProduct>>(json)
                ?? throw new InvalidOperationException("Failed to deserialize product list");
        }

        // Customer endpoints
        public async Task<PolarCustomer> CreateCustomerAsync(string email, string? name = null)
        {
            var request = new CreateCustomerRequest
            {
                Email = email,
                Name = name ?? email.Split('@')[0],
                OrganizationId = _organizationId
            };

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/v1/customers", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to create customer: {response.StatusCode} - {error}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<PolarCustomer>(responseContent)
                ?? throw new InvalidOperationException("Failed to deserialize PolarCustomer");
        }

        public async Task<PolarListResponse<PolarCustomer>> ListCustomersAsync()
        {
            var response = await _httpClient.GetAsync($"/v1/customers?organization_id={_organizationId}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<PolarListResponse<PolarCustomer>>(json)
                ?? throw new InvalidOperationException("Failed to deserialize customer list");
        }

        public async Task<PolarCustomer> GetCustomerAsync(string customerId)
        {
            var response = await _httpClient.GetAsync($"/v1/customers/{customerId}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<PolarCustomer>(json)
                ?? throw new InvalidOperationException("Failed to deserialize PolarCustomer");
        }

        // Subscription endpoints
        public async Task<PolarSubscription> CreateSubscriptionAsync(string customerId, string? priceId = null)
        {
            priceId ??= _priceId;

            var request = new CreateSubscriptionRequest
            {
                CustomerId = customerId,
                ProductPriceId = priceId
            };

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/v1/subscriptions", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to create subscription: {response.StatusCode} - {error}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<PolarSubscription>(responseContent)
                ?? throw new InvalidOperationException("Failed to deserialize PolarSubscription");
        }

        public async Task<PolarListResponse<PolarSubscription>> ListSubscriptionsAsync()
        {
            var response = await _httpClient.GetAsync($"/v1/subscriptions?organization_id={_organizationId}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<PolarListResponse<PolarSubscription>>(json)
                ?? throw new InvalidOperationException("Failed to deserialize subscription list");
        }

        public async Task<PolarSubscription> GetSubscriptionAsync(string subscriptionId)
        {
            var response = await _httpClient.GetAsync($"/v1/subscriptions/{subscriptionId}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<PolarSubscription>(json)
                ?? throw new InvalidOperationException("Failed to deserialize PolarSubscription");
        }

        public async Task<bool> CancelSubscriptionAsync(string subscriptionId)
        {
            var response = await _httpClient.DeleteAsync($"/v1/subscriptions/{subscriptionId}");
            return response.IsSuccessStatusCode;
        }

        // Checkout endpoints
        public async Task<PolarCheckout> CreateCheckoutAsync(string? customerEmail = null, string? successUrl = null)
        {
            var request = new CreateCheckoutRequest
            {
                ProductPriceId = _priceId,
                SuccessUrl = successUrl ?? "https://example.com/success",
                CustomerEmail = customerEmail ?? string.Empty,
                Metadata = new Dictionary<string, string>
                {
                    { "source", "polar-net" },
                    { "timestamp", DateTime.UtcNow.ToString("O") }
                }
            };

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/v1/checkouts/custom", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to create checkout: {response.StatusCode} - {error}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<PolarCheckout>(responseContent)
                ?? throw new InvalidOperationException("Failed to deserialize PolarCheckout");
        }

        public async Task<PolarCheckout> GetCheckoutAsync(string checkoutId)
        {
            var response = await _httpClient.GetAsync($"/v1/checkouts/custom/{checkoutId}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<PolarCheckout>(json)
                ?? throw new InvalidOperationException("Failed to deserialize PolarCheckout");
        }

        // Order endpoints
        public async Task<PolarListResponse<PolarOrder>> ListOrdersAsync()
        {
            var response = await _httpClient.GetAsync($"/v1/orders?organization_id={_organizationId}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<PolarListResponse<PolarOrder>>(json)
                ?? throw new InvalidOperationException("Failed to deserialize order list");
        }

        // Benefit endpoints
        public async Task<PolarListResponse<PolarBenefit>> ListBenefitsAsync()
        {
            var response = await _httpClient.GetAsync($"/v1/benefits?organization_id={_organizationId}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<PolarListResponse<PolarBenefit>>(json)
                ?? throw new InvalidOperationException("Failed to deserialize benefit list");
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}