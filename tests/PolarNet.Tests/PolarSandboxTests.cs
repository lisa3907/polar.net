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
#if DEBUG
                .AddJsonFile("appsettings.Development.json", optional: true)
#else
                .AddJsonFile("appsettings.json", optional: false)
#endif
                .AddEnvironmentVariables(prefix: "POLAR_TEST_")
                .Build();

            var section = _config.GetSection("PolarSettings");
            var accessToken = section["AccessToken"] ?? string.Empty;
            var useSandbox = string.Equals(section["UseSandbox"], "true", StringComparison.OrdinalIgnoreCase);
            var baseUrl = (useSandbox ? section["SandboxApiUrl"] : section["ProductionApiUrl"]) ?? string.Empty;
            OrganizationId = section["OrganizationId"];
            ProductId = section["ProductId"];
            PriceId = section["PriceId"];
            WebhookBaseUrl = section["WebhookBaseUrl"];

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

            // ========== NEW: Payment API Tests ==========
            var payments = await client.ListPaymentsAsync();
            Assert.NotNull(payments);
            Assert.NotNull(payments.Items);

            if (payments.Items.Count > 0)
            {
                var firstPayment = payments.Items[0];
                var payment = await client.GetPaymentAsync(firstPayment.Id);
                Assert.NotNull(payment);
                Assert.Equal(firstPayment.Id, payment.Id);

                // Test filtering by order
                if (!string.IsNullOrEmpty(payment.OrderId))
                {
                    var orderPayments = await client.ListPaymentsByOrderAsync(payment.OrderId);
                    Assert.NotNull(orderPayments);
                    Assert.Contains(orderPayments.Items, p => p.Id == payment.Id);
                }
            }

            // ========== NEW: Refund API Tests ==========
            var refunds = await client.ListRefundsAsync();
            Assert.NotNull(refunds);
            Assert.NotNull(refunds.Items);

            if (refunds.Items.Count > 0)
            {
                var firstRefund = refunds.Items[0];
                var refund = await client.GetRefundAsync(firstRefund.Id);
                Assert.NotNull(refund);
                Assert.Equal(firstRefund.Id, refund.Id);
            }

            // ========== NEW: Webhook Management API Tests ==========
            var webhooks = await client.ListWebhookEndpointsAsync();
            Assert.NotNull(webhooks);
            Assert.NotNull(webhooks.Items);

            if (webhooks.Items.Count > 0)
            {
                var firstWebhook = webhooks.Items[0];
                var webhook = await client.GetWebhookEndpointAsync(firstWebhook.Id);
                Assert.NotNull(webhook);
                Assert.Equal(firstWebhook.Id, webhook.Id);
                Assert.NotNull(webhook.Url);
                Assert.NotNull(webhook.Events);
            }
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
        public async Task Payments_List_And_Get_Work()
        {
            var client = GetClientOrSkip();

            var payments = await client.ListPaymentsAsync(page: 1, limit: 10);
            Assert.NotNull(payments);
            Assert.NotNull(payments.Items);
            Assert.True(payments.Pagination.TotalCount >= 0);

            if (payments.Items.Count > 0)
            {
                var firstPayment = payments.Items[0];
                var payment = await client.GetPaymentAsync(firstPayment.Id);
                Assert.NotNull(payment);
                Assert.Equal(firstPayment.Id, payment.Id);
                Assert.False(string.IsNullOrWhiteSpace(payment.Status));
            }
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        public async Task Refunds_List_And_Get_Work()
        {
            var client = GetClientOrSkip();

            var refunds = await client.ListRefundsAsync(page: 1, limit: 10);
            Assert.NotNull(refunds);
            Assert.NotNull(refunds.Items);
            Assert.True(refunds.Pagination.TotalCount >= 0);

            if (refunds.Items.Count > 0)
            {
                var firstRefund = refunds.Items[0];
                var refund = await client.GetRefundAsync(firstRefund.Id);
                Assert.NotNull(refund);
                Assert.Equal(firstRefund.Id, refund.Id);
                Assert.False(string.IsNullOrWhiteSpace(refund.Status));
                Assert.False(string.IsNullOrWhiteSpace(refund.Reason));
            }
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        public async Task WebhookEndpoints_List_And_Get_Work()
        {
            var client = GetClientOrSkip();

            var webhooks = await client.ListWebhookEndpointsAsync(page: 1, limit: 10);
            Assert.NotNull(webhooks);
            Assert.NotNull(webhooks.Items);
            Assert.True(webhooks.Pagination.TotalCount >= 0);

            if (webhooks.Items.Count > 0)
            {
                var firstWebhook = webhooks.Items[0];
                var webhook = await client.GetWebhookEndpointAsync(firstWebhook.Id);
                Assert.NotNull(webhook);
                Assert.Equal(firstWebhook.Id, webhook.Id);
                Assert.False(string.IsNullOrWhiteSpace(webhook.Url));
                Assert.NotNull(webhook.Events);
                Assert.True(webhook.Events.Count > 0);
            }
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        public async Task WebhookEndpoints_Create_Update_Delete_Work()
        {
            var client = GetClientOrSkip();

            // Create a test webhook endpoint
            var testUrl = $"https://webhook.site/{Guid.NewGuid():N}";
            var events = new List<string> { "order.created", "subscription.created" };

            var created = await client.CreateWebhookEndpointAsync(testUrl, events, "test-secret");
            Assert.NotNull(created);
            Assert.False(string.IsNullOrWhiteSpace(created.Id));
            Assert.Equal(testUrl, created.Url);
            Assert.Equal(2, created.Events.Count);

            try
            {
                // Update the webhook endpoint
                var updateRequest = new PolarNet.Models.Resources.UpdateWebhookEndpointRequest
                {
                    Events = new List<string> { "order.created", "subscription.created", "payment.succeeded" }
                };

                var updated = await client.UpdateWebhookEndpointAsync(created.Id, updateRequest);
                Assert.NotNull(updated);
                Assert.Equal(3, updated.Events.Count);

                // Test the webhook endpoint
                var testResult = await client.TestWebhookEndpointAsync(created.Id);
                // Note: Test might fail if the URL is not reachable, but the method should work
                // testResult is a bool (value type), no need for null check
            }
            finally
            {
                // Clean up: Delete the webhook endpoint
                var deleted = await client.DeleteWebhookEndpointAsync(created.Id);
                Assert.True(deleted);
            }
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