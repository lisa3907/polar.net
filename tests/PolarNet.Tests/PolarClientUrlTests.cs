using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PolarNet.Services;
using Xunit;

namespace PolarNet.Tests
{
    internal sealed class FakeHandler : HttpMessageHandler
    {
        public string LastRequestUri { get; private set; } = string.Empty;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequestUri = request.RequestUri?.ToString() ?? string.Empty;

            // Return minimal valid JSON per endpoint shape to satisfy deserialization in client methods
            string json;
            if (LastRequestUri.Contains("/v1/products/")) json = "{\"id\":\"p_1\",\"name\":\"Test\"}";
            else if (LastRequestUri.Contains("/v1/products")) json = "{\"items\":[],\"pagination\":{\"total_count\":0,\"max_page\":0}}";
            else if (LastRequestUri.Contains("/v1/prices/")) json = "{\"id\":\"price_1\",\"product_id\":\"p_1\",\"amount_type\":\"fixed\",\"type\":\"recurring\",\"recurring_interval\":\"month\",\"price_currency\":\"USD\",\"is_archived\":false}";
            else if (LastRequestUri.Contains("/v1/prices")) json = "{\"items\":[{\"id\":\"price_1\",\"product_id\":\"p_1\",\"amount_type\":\"fixed\",\"type\":\"recurring\",\"recurring_interval\":\"month\",\"price_currency\":\"USD\",\"is_archived\":false}],\"pagination\":{\"total_count\":1,\"max_page\":1}}";
            else if (LastRequestUri.Contains("/v1/orders/")) json = "{\"id\":\"ord_1\",\"customer_id\":\"c_1\",\"product_id\":\"p_1\",\"product_price_id\":\"price_1\",\"amount\":100,\"currency\":\"USD\",\"created_at\":\"2025-01-01T00:00:00Z\"}";
            else if (LastRequestUri.Contains("/v1/orders")) json = "{\"items\":[{\"id\":\"ord_1\",\"customer_id\":\"c_1\",\"product_id\":\"p_1\",\"product_price_id\":\"price_1\",\"amount\":100,\"currency\":\"USD\",\"created_at\":\"2025-01-01T00:00:00Z\"}],\"pagination\":{\"total_count\":1,\"max_page\":1}}";
            else if (LastRequestUri.Contains("/v1/customers/") && LastRequestUri.EndsWith("/state")) json = "{\"id\":\"c_1\",\"email\":\"e@example.com\",\"name\":\"N\",\"organization_id\":\"org_1\",\"active_subscriptions\":[],\"granted_benefits\":[],\"active_meters\":[]}";
            else json = "{}";

            var resp = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            };
            return Task.FromResult(resp);
        }
    }

    public class PolarClientUrlTests
    {
        private readonly string _baseUrl;
        private readonly string _orgId;

        public PolarClientUrlTests()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
            var polar = config.GetSection("PolarSettings");
            var useSandbox = string.Equals(polar["UseSandbox"], "true", StringComparison.OrdinalIgnoreCase);
            _baseUrl = (useSandbox ? polar["SandboxApiUrl"] : polar["ProductionApiUrl"]) ?? "";
            _orgId = polar["OrganizationId"] ?? "org_1";
        }
        [Fact]
        public async Task ListProducts_Uses_Organization_Query()
        {
            var handler = new FakeHandler();
            var client = new PolarClient(new PolarClientOptions { AccessToken = "x", BaseUrl = _baseUrl, OrganizationId = _orgId }, handler);
            await client.ListProductsAsync();
            Assert.Contains($"/v1/products?organization_id={_orgId}", handler.LastRequestUri);
        }

        [Fact]
        public async Task ListPrices_Adds_Optional_ProductId_Filter()
        {
            var handler = new FakeHandler();
            var client = new PolarClient(new PolarClientOptions { AccessToken = "x", BaseUrl = _baseUrl, OrganizationId = _orgId }, handler);
            await client.ListPricesAsync("p_1");
            Assert.Contains($"/v1/prices?organization_id={_orgId}&product_id=p_1", handler.LastRequestUri);
        }

        [Fact]
        public async Task GetCustomerState_Calls_State_Path()
        {
            var handler = new FakeHandler();
            var client = new PolarClient(new PolarClientOptions { AccessToken = "x", BaseUrl = _baseUrl }, handler);
            await client.GetCustomerStateAsync("c_1");
            Assert.EndsWith("/v1/customers/c_1/state", handler.LastRequestUri);
        }

        [Fact]
        public async Task CreateSubscription_Throws_When_No_PriceId()
        {
            var client = new PolarClient(new PolarClientOptions { AccessToken = "x", BaseUrl = _baseUrl });
            await Assert.ThrowsAsync<ArgumentException>(() => client.CreateSubscriptionAsync("c_1"));
        }
    }
}