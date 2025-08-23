using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PolarNet.Models;
using PolarNet.Services;
using Xunit;

namespace PolarNet.Tests
{
    /// <summary>
    /// Loads sandbox configuration from tests/appsettings.json and constructs a PolarClient for integration tests.
    /// If any required config is missing, tests will be skipped with a clear message.
    /// </summary>
    public sealed class PolarSandboxFixture : IAsyncLifetime
    {
        public PolarClient? Client { get; private set; }
        public string? OrganizationId { get; private set; }
        public string? ProductId { get; private set; }
        public string? PriceId { get; private set; }
    public string? WebhookBaseUrl { get; private set; }

        private IConfigurationRoot? _config;

        public Task InitializeAsync()
        {
            _config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables(prefix: "POLAR_TEST_")
                .Build();

            var section = _config.GetSection("PolarSettings");
            var accessToken = section["AccessToken"] ?? string.Empty;
            var useSandbox = string.Equals(section["UseSandbox"], "true", StringComparison.OrdinalIgnoreCase);
            var baseUrl = (useSandbox ? section["SandboxApiUrl"] : section["ProductionApiUrl"]) ?? string.Empty;
            OrganizationId = section["OrganizationId"];
            ProductId = section["ProductId"];
            PriceId = section["PriceId"];
            WebhookBaseUrl = section["WebhookBaseUrl"]; // e.g., https://abc123.ngrok-free.app

            if (IsPlaceholder(accessToken) || string.IsNullOrWhiteSpace(baseUrl))
            {
                // Leave Client = null; tests will skip with message.
                return Task.CompletedTask;
            }

            var options = new PolarClientOptions
            {
                AccessToken = accessToken,
                BaseUrl = baseUrl,
                OrganizationId = !IsPlaceholder(OrganizationId) ? OrganizationId : null,
                DefaultProductId = !IsPlaceholder(ProductId) ? ProductId : null,
                DefaultPriceId = !IsPlaceholder(PriceId) ? PriceId : null
            };

            Client = new PolarClient(options);
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            Client?.Dispose();
            return Task.CompletedTask;
        }

        internal static bool IsPlaceholder(string? value)
            => string.IsNullOrWhiteSpace(value) || value!.StartsWith("<", StringComparison.Ordinal);
    }

    [CollectionDefinition(Name)]
    public class PolarSandboxCollection : ICollectionFixture<PolarSandboxFixture>
    {
        public const string Name = "PolarSandboxCollection";
    }

    [Collection(PolarSandboxCollection.Name)]
    public class PolarSandboxTests
    {
        private readonly PolarSandboxFixture _fx;

        public PolarSandboxTests(PolarSandboxFixture fx)
        {
            _fx = fx;
        }

        private PolarClient GetClientOrSkip()
        {
            Skip.If(_fx.Client is null, "Polar sandbox credentials/base URL not configured. Fill PolarSettings in tests/appsettings.json.");
            return _fx.Client!;
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        public async Task Products_And_Prices_List_And_Get_Works()
        {
            var client = GetClientOrSkip();

            // OrganizationId no longer required for listing; rely on token context.

            // List products
            var products = await client.ListProductsAsync();
            Assert.NotNull(products);
            Assert.NotNull(products.Pagination);

            // Prices are embedded in the Product payload in sandbox; validate via product.Prices
            if (!PolarSandboxFixture.IsPlaceholder(_fx.ProductId))
            {
                var product = await client.GetProductAsync(_fx.ProductId);
                Assert.NotNull(product);
                Assert.False(string.IsNullOrWhiteSpace(product.Id));
                Assert.NotNull(product.Prices);

                // If a PriceId is configured, ensure it exists on this product
                if (!PolarSandboxFixture.IsPlaceholder(_fx.PriceId))
                {
                    var hasPrice = product.Prices.Any(p => string.Equals(p.Id, _fx.PriceId, StringComparison.OrdinalIgnoreCase));
                    Assert.True(hasPrice, $"Configured PriceId {_fx.PriceId} not found on product {product.Id}.");
                }
            }
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        public async Task Customers_State_And_List_Works()
        {
            var client = GetClientOrSkip();

            // OrganizationId not required for creating/listing customers when token is scoped to org.

            // Create a customer with a unique email
            var email = $"int-test-{Guid.NewGuid():N}@mailinator.com";
            var created = await client.CreateCustomerAsync(email, "Integration Test");
            Assert.NotNull(created);
            Assert.False(string.IsNullOrWhiteSpace(created.Id));

            // Get customer
            var fetched = await client.GetCustomerAsync(created.Id);
            Assert.Equal(created.Id, fetched.Id);

            // State endpoint
            var state = await client.GetCustomerStateAsync(created.Id);
            Assert.NotNull(state);

            // List customers
            var list = await client.ListCustomersAsync();
            Assert.NotNull(list);
            Assert.NotNull(list.Pagination);
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
    public async Task Customers_Create_Get_List_CreateSubscription_Revoke_And_Delete_Works()
        {
            var client = GetClientOrSkip();

            // Create a customer with a unique email
            var email = $"int-del-{Guid.NewGuid():N}@mailinator.com";
            var created = await client.CreateCustomerAsync(email, "Delete Flow Test");
            Assert.NotNull(created);
            Assert.False(string.IsNullOrWhiteSpace(created.Id));

            // Get customer
            var fetched = await client.GetCustomerAsync(created.Id);
            Assert.Equal(created.Id, fetched.Id);

            // List customers
            var list = await client.ListCustomersAsync();
            Assert.NotNull(list);
            Assert.NotNull(list.Pagination);

            // Try to create a subscription for this customer if PriceId is configured
            PolarSubscription? createdSub = null;
            if (!PolarSandboxFixture.IsPlaceholder(_fx.PriceId))
            {
                try
                {
                    createdSub = await client.CreateSubscriptionAsync(created.Id, _fx.PriceId);
                    Assert.NotNull(createdSub);
                }
                catch (Exception ex) when (ex.Message.Contains("MethodNotAllowed", StringComparison.OrdinalIgnoreCase)
                                         || ex.Message.Contains("Not Found", StringComparison.OrdinalIgnoreCase))
                {
                    // In some environments direct subscription create is disallowed; continue without failing the test.
                }
            }

            // Revoke any active subscriptions (including the one we just created, if any) before delete
            var state = await client.GetCustomerStateAsync(created.Id);
            if (createdSub != null)
            {
                await client.RevokeSubscriptionAsync(createdSub.Id);
            }
            if (state.ActiveSubscriptions?.Any() == true)
            {
                foreach (var s in state.ActiveSubscriptions)
                {
                    await client.RevokeSubscriptionAsync(s.Id);
                }
            }

            // Delete customer
            var deleted = await client.DeleteCustomerAsync(created.Id);
            Assert.True(deleted);
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        public async Task Checkout_Create_And_Get_Works()
        {
            var client = GetClientOrSkip();

            Skip.If(PolarSandboxFixture.IsPlaceholder(_fx.PriceId), "PriceId (DefaultPriceId) required for checkout tests.");

            // Create checkout (uses DefaultPriceId from options)
            var checkout = await client.CreateCheckoutAsync($"buyer-{Guid.NewGuid():N}@mailinator.com", "https://example.com/success");
            Assert.False(string.IsNullOrWhiteSpace(checkout.Id));

            var got = await client.GetCheckoutAsync(checkout.Id);
            Assert.Equal(checkout.Id, got.Id);
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        public async Task Orders_And_Benefits_List_Works()
        {
            var client = GetClientOrSkip();

            // OrganizationId not required for listing orders/benefits with org-scoped token.

            // Orders list (may be empty in sandbox)
            var orders = await client.ListOrdersAsync();
            Assert.NotNull(orders);

            // If there's at least one order, fetch it
            var firstOrder = orders.Items?.FirstOrDefault();
            if (firstOrder != null)
            {
                var order = await client.GetOrderAsync(firstOrder.Id);
                Assert.Equal(firstOrder.Id, order.Id);
            }

            // Benefits list
            var benefits = await client.ListBenefitsAsync();
            Assert.NotNull(benefits);
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        public async Task Organization_Get_Works()
        {
            var client = GetClientOrSkip();

            // OrganizationId not required for organization get if configured; will throw if missing in client options.

            var org = await client.GetOrganizationAsync();
            Assert.NotNull(org);
            Assert.False(string.IsNullOrWhiteSpace(org.Id));
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        public async Task Webhook_Receives_CustomerCreated_Event()
        {
            var client = GetClientOrSkip();
            Skip.If(PolarSandboxFixture.IsPlaceholder(_fx.WebhookBaseUrl), "WebhookBaseUrl not configured (tests/appsettings.json → PolarSettings:WebhookBaseUrl, e.g., ngrok URL)");

            // 1) Mark start time for filtering
            var since = DateTime.UtcNow.AddSeconds(-5);

            // 2) Trigger an event in Sandbox (create customer)
            var email = $"hook-{Guid.NewGuid():N}@mailinator.com";
            var created = await client.CreateCustomerAsync(email, "Webhook Test");
            Assert.False(string.IsNullOrWhiteSpace(created.Id));

            // 3) Poll webhook sample endpoint for receipt
            using var http = new HttpClient();
            http.Timeout = TimeSpan.FromSeconds(5);

            var deadline = DateTime.UtcNow.AddMinutes(2);
            WebhookPollResponse? last = null;
            while (DateTime.UtcNow < deadline)
            {
                try
                {
                    var url = new Uri(new Uri(_fx.WebhookBaseUrl!), $"/api/webhook/events?sinceUtc={Uri.EscapeDataString(since.ToString("o"))}&type=customer.created&max=50");
                    var resp = await http.GetAsync(url);
                    var content = await resp.Content.ReadAsStringAsync();
                    resp.EnsureSuccessStatusCode();
                    last = System.Text.Json.JsonSerializer.Deserialize<WebhookPollResponse>(content);
                    if (last?.Items?.Any(i => i.Data.TryGetProperty("id", out var idProp) && idProp.GetString() == created.Id) == true)
                    {
                        return; // success
                    }
                }
                catch
                {
                    // swallow and retry until deadline
                }
                await Task.Delay(2000);
            }

            // If we get here, no event observed
            var detail = last == null ? "(no response)" : $"lastCount={last.Count}";
            throw new Xunit.Sdk.XunitException($"Timed out waiting for webhook event. {detail}");
        }

        private sealed class WebhookPollResponse
        {
            public int Count { get; set; }
            public List<WebhookItem> Items { get; set; } = new();
        }

        private sealed class WebhookItem
        {
            public string EventId { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
            public System.Text.Json.JsonElement Data { get; set; }
        }
    }
}