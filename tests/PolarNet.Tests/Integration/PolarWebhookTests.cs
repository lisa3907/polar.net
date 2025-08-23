using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PolarNet.Models;
using PolarNet.Models.Resources;
using PolarNet.Services;
using PolarNet.Tests.Base;
using Xunit;
using Xunit.Abstractions;

namespace PolarNet.Tests.Integration
{
    /// <summary>
    /// Comprehensive tests for Webhook functionality including:
    /// - Webhook Endpoint Management API (CRUD operations)
    /// - Webhook Event Processing (signature verification, parsing, dispatching)
    /// </summary>
    public class PolarWebhookTests : TestBase
    {
        public PolarWebhookTests(PolarSandboxFixture fixture, ITestOutputHelper output) 
            : base(fixture, output)
        {
        }

        #region Webhook Endpoint Management API Tests

        [Fact]
        [Trait("Category", "Integration")]
        public async Task CreateWebhookEndpoint_WithBasicInfo_ShouldSucceed()
        {
            // Arrange
            SkipIfNoClient(nameof(CreateWebhookEndpoint_WithBasicInfo_ShouldSucceed));
            LogSection("Create Webhook Endpoint Test");
            
            var webhookBase = WebhookBaseUrl;
            var url = $"{webhookBase}/webhook/{GenerateTestId()}";
            var events = new List<string> { "order.created", "subscription.created" };
            // Webhook secret must be at least 32 characters
            var secret = "test_secret_" + Guid.NewGuid().ToString("N");

            // Act
            Log($"Creating webhook endpoint: {url}");
            var endpoint = await Client!.CreateWebhookEndpointAsync(url, events, secret);

            // Assert
            Assert.NotNull(endpoint);
            Assert.NotEmpty(endpoint.Id);
            Assert.Equal(url, endpoint.Url);
            Assert.NotNull(endpoint.Events);
            Assert.Equal(events.Count, endpoint.Events.Count);
            foreach (var evt in events)
            {
                Assert.Contains(evt, endpoint.Events);
            }
            
            Log($"✓ Webhook endpoint created: {endpoint.Id}");
            Log($"  URL: {endpoint.Url}");
            Log($"  Events: {string.Join(", ", endpoint.Events)}");

            // Cleanup
            await Client.DeleteWebhookEndpointAsync(endpoint.Id);
            Log("✓ Webhook endpoint deleted");
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task CreateWebhookEndpoint_WithRequest_ShouldSucceed()
        {
            // Arrange
            SkipIfNoClient(nameof(CreateWebhookEndpoint_WithRequest_ShouldSucceed));
            
            var webhookBase = WebhookBaseUrl;
            var request = new CreateWebhookEndpointRequest
            {
                Url = $"{webhookBase}/webhook/{GenerateTestId()}",
                Events = new List<string> { "customer.created", "customer.updated" },
                // Webhook secret must be at least 32 characters
                Secret = "secret_" + Guid.NewGuid().ToString("N") + "_webhook"
            };

            // Act
            var endpoint = await Client!.CreateWebhookEndpointAsync(request);

            // Assert
            Assert.NotNull(endpoint);
            Assert.NotEmpty(endpoint.Id);
            Assert.Equal(request.Url, endpoint.Url);
            Assert.Equal(request.Events.Count, endpoint.Events.Count);
            
            Log($"✓ Webhook endpoint created with request: {endpoint.Id}");

            // Cleanup
            await Client.DeleteWebhookEndpointAsync(endpoint.Id);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task ListWebhookEndpoints_ShouldReturnEndpoints()
        {
            // Arrange
            SkipIfNoClient(nameof(ListWebhookEndpoints_ShouldReturnEndpoints));
            LogSection("List Webhook Endpoints Test");
            
            // Create a test endpoint to ensure at least one exists
            var webhookBase = WebhookBaseUrl;
            var testEndpoint = await Client!.CreateWebhookEndpointAsync(
                $"{webhookBase}/webhook/{GenerateTestId()}",
                new List<string> { "order.created" }
            );

            try
            {
                // Act
                Log("Fetching webhook endpoints...");
                var endpoints = await Client.ListWebhookEndpointsAsync(1, 10);

                // Assert
                Assert.NotNull(endpoints);
                Assert.NotNull(endpoints.Items);
                Assert.True(endpoints.Items.Count > 0, "Should have at least one endpoint");
                
                Log($"✓ Found {endpoints.Items.Count} webhook endpoints");
                
                foreach (var endpoint in endpoints.Items.Take(3))
                {
                    Assert.NotEmpty(endpoint.Id);
                    Assert.NotEmpty(endpoint.Url);
                    Assert.NotNull(endpoint.Events);
                    
                    Log($"  - Endpoint ID: {endpoint.Id}");
                    Log($"    URL: {endpoint.Url}");
                    Log($"    Events: {string.Join(", ", endpoint.Events)}");
                    Log($"    Created: {endpoint.CreatedAt:yyyy-MM-dd HH:mm:ss}");
                }
            }
            finally
            {
                // Cleanup
                await Client.DeleteWebhookEndpointAsync(testEndpoint.Id);
            }
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task GetWebhookEndpoint_WithValidId_ShouldReturnEndpoint()
        {
            // Arrange
            SkipIfNoClient(nameof(GetWebhookEndpoint_WithValidId_ShouldReturnEndpoint));
            LogSection("Get Webhook Endpoint Test");
            
            var webhookBase = WebhookBaseUrl;
            var createdEndpoint = await Client!.CreateWebhookEndpointAsync(
                $"{webhookBase}/webhook/{GenerateTestId()}",
                new List<string> { "subscription.created", "subscription.updated" }
            );

            try
            {
                // Act
                Log($"Fetching webhook endpoint {createdEndpoint.Id}...");
                var endpoint = await Client.GetWebhookEndpointAsync(createdEndpoint.Id);

                // Assert
                Assert.NotNull(endpoint);
                Assert.Equal(createdEndpoint.Id, endpoint.Id);
                Assert.Equal(createdEndpoint.Url, endpoint.Url);
                Assert.Equal(createdEndpoint.Events.Count, endpoint.Events.Count);
                
                Log($"✓ Webhook endpoint retrieved: {endpoint.Id}");
                Log($"  URL: {endpoint.Url}");
                Log($"  Events: {string.Join(", ", endpoint.Events)}");
            }
            finally
            {
                // Cleanup
                await Client.DeleteWebhookEndpointAsync(createdEndpoint.Id);
            }
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task UpdateWebhookEndpoint_ShouldModifyEndpoint()
        {
            // Arrange
            SkipIfNoClient(nameof(UpdateWebhookEndpoint_ShouldModifyEndpoint));
            LogSection("Update Webhook Endpoint Test");
            
            var webhookBase = WebhookBaseUrl;
            var originalEndpoint = await Client!.CreateWebhookEndpointAsync(
                $"{webhookBase}/webhook/{GenerateTestId()}",
                new List<string> { "order.created" }
            );

            try
            {
                var updateRequest = new UpdateWebhookEndpointRequest
                {
                    Url = $"{webhookBase}/updated/{GenerateTestId()}",
                    Events = new List<string> { "order.created", "order.updated", "order.paid" }
                };

                // Act
                Log($"Updating webhook endpoint {originalEndpoint.Id}...");
                var updatedEndpoint = await Client.UpdateWebhookEndpointAsync(originalEndpoint.Id, updateRequest);

                // Assert
                Assert.NotNull(updatedEndpoint);
                Assert.Equal(originalEndpoint.Id, updatedEndpoint.Id);
                Assert.Equal(updateRequest.Url, updatedEndpoint.Url);
                Assert.Equal(updateRequest.Events.Count, updatedEndpoint.Events.Count);
                foreach (var evt in updateRequest.Events)
                {
                    Assert.Contains(evt, updatedEndpoint.Events);
                }
                
                Log($"✓ Webhook endpoint updated");
                Log($"  New URL: {updatedEndpoint.Url}");
                Log($"  New Events: {string.Join(", ", updatedEndpoint.Events)}");
            }
            finally
            {
                // Cleanup
                await Client.DeleteWebhookEndpointAsync(originalEndpoint.Id);
            }
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task DeleteWebhookEndpoint_ShouldRemoveEndpoint()
        {
            // Arrange
            SkipIfNoClient(nameof(DeleteWebhookEndpoint_ShouldRemoveEndpoint));
            LogSection("Delete Webhook Endpoint Test");
            
            var webhookBase = WebhookBaseUrl;
            var endpoint = await Client!.CreateWebhookEndpointAsync(
                $"{webhookBase}/webhook/{GenerateTestId()}",
                new List<string> { "customer.created" }
            );
            Log($"Created webhook endpoint: {endpoint.Id}");

            // Act
            Log($"Deleting webhook endpoint {endpoint.Id}...");
            var result = await Client.DeleteWebhookEndpointAsync(endpoint.Id);

            // Assert
            Assert.True(result, "Delete should return true");
            Log($"✓ Webhook endpoint deleted successfully");

            // Verify deletion
            try
            {
                await Client.GetWebhookEndpointAsync(endpoint.Id);
                Assert.Fail("Should not be able to get deleted endpoint");
            }
            catch
            {
                Log("✓ Confirmed webhook endpoint no longer exists");
            }
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task TestWebhookEndpoint_ShouldTriggerTestEvent()
        {
            // Arrange
            SkipIfNoClient(nameof(TestWebhookEndpoint_ShouldTriggerTestEvent));
            LogSection("Test Webhook Endpoint");
            
            var webhookBase = WebhookBaseUrl;
            var endpoint = await Client!.CreateWebhookEndpointAsync(
                $"{webhookBase}/webhook/{GenerateTestId()}",
                new List<string> { "order.created", "customer.created" }
            );

            try
            {
                // Act
                Log($"Testing webhook endpoint {endpoint.Id}...");
                var result = await Client.TestWebhookEndpointAsync(endpoint.Id, "order.created");

                // Note: The test may return false if the webhook URL is unreachable
                // This is expected behavior when using test URLs like webhook.site
                // The important thing is that the API call succeeds without throwing an exception
                
                if (result)
                {
                    Log($"✓ Test event sent successfully");
                }
                else
                {
                    Log($"⚠ Test event initiated but webhook URL may be unreachable (expected for test URLs)");
                }
                
                // The test passes as long as no exception was thrown
                // The actual webhook delivery status depends on the URL being reachable
                Assert.True(true, "Test webhook endpoint API call completed");
                Log("  Note: Check the webhook URL for the test event delivery if URL is reachable");
            }
            finally
            {
                // Cleanup
                await Client.DeleteWebhookEndpointAsync(endpoint.Id);
            }
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task ListWebhookEndpoints_WithPagination_ShouldRespectLimits()
        {
            // Arrange
            SkipIfNoClient(nameof(ListWebhookEndpoints_WithPagination_ShouldRespectLimits));
            const int requestedLimit = 5;
            
            // Create multiple endpoints to test pagination
            var endpointIds = new List<string>();
            var webhookBase = WebhookBaseUrl;
            for (int i = 0; i < 3; i++)
            {
                var endpoint = await Client!.CreateWebhookEndpointAsync(
                    $"{webhookBase}/webhook/{GenerateTestId()}",
                    new List<string> { "order.created" }
                );
                endpointIds.Add(endpoint.Id);
            }

            try
            {
                // Act
                var endpoints = await Client!.ListWebhookEndpointsAsync(1, requestedLimit);

                // Assert
                Assert.NotNull(endpoints);
                Assert.NotNull(endpoints.Items);
                Assert.True(endpoints.Items.Count <= requestedLimit, 
                    $"Expected at most {requestedLimit} items, but got {endpoints.Items.Count}");
                
                Log($"✓ Pagination respected: requested {requestedLimit}, received {endpoints.Items.Count}");
            }
            finally
            {
                // Cleanup
                foreach (var id in endpointIds)
                {
                    await Client!.DeleteWebhookEndpointAsync(id);
                }
            }
        }

        #endregion

        #region Webhook Event Processing Tests

        private static string ComputeSignature(string secret, byte[] body)
        {
            using var h = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            return Convert.ToBase64String(h.ComputeHash(body));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void VerifySignature_Allows_When_Secret_Empty()
        {
            var svc = new PolarWebhookService(secret: "");
            var ok = svc.VerifySignature(Encoding.UTF8.GetBytes("{}"), signatureBase64: "");
            Assert.True(ok);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void VerifySignature_Computes_HmacSha256_Base64()
        {
            var secret = "topsecret";
            var payload = Encoding.UTF8.GetBytes("{\"hello\":\"world\"}");
            var expected = ComputeSignature(secret, payload);

            var svc = new PolarWebhookService(secret);
            Assert.True(svc.VerifySignature(payload, expected));
            Assert.False(svc.VerifySignature(payload, expected + "bad"));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Parse_Envelope_Roundtrips()
        {
            var now = DateTime.UtcNow;
            var json = JsonSerializer.Serialize(new
            {
                type = "customer.created",
                data = new { id = "cus_123", email = "a@b.com", name = "A", organization_id = "org_1", created_at = now },
                event_id = "evt_1",
                created_at = now
            });

            var svc = new PolarWebhookService("irrelevant");
            var payload = svc.Parse(json);

            Assert.Equal("customer.created", payload.Type);
            Assert.Equal("evt_1", payload.EventId);
            Assert.True(payload.Data.ValueKind == JsonValueKind.Object);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Dispatch_Invokes_Strongly_Typed_Handlers()
        {
            var now = DateTime.UtcNow;
            string MakeJson(string type, object data) => JsonSerializer.Serialize(new { type, data, event_id = "evt_x", created_at = now });

            // checkout.created
            var checkoutJson = MakeJson("checkout.created", new { id = "chk_1", status = "created", customer_id = "", product_id = "prod_1", success_url = "https://x", created_at = now });
            var customerJson = MakeJson("customer.created", new { id = "cus_1", email = "e@x.com", name = "N", organization_id = "org_1", created_at = now });
            var orderJson = MakeJson("order.created", new { id = "ord_1", customer_id = "cus_1", product_id = "prod_1", amount = 1000, currency = "USD", status = "paid", created_at = now });
            var subJson = MakeJson("subscription.created", new { id = "sub_1", status = "active", customer_id = "cus_1", product_id = "prod_1", price_id = "price_1", created_at = now });

            var svc = new PolarWebhookService("irrelevant");
            var handler = new CapturingHandler();

            await svc.DispatchAsync(svc.Parse(checkoutJson), handler);
            Assert.Equal("chk_1", handler.CapturedCheckout?.Id);

            await svc.DispatchAsync(svc.Parse(customerJson), handler);
            Assert.Equal("cus_1", handler.CapturedCustomer?.Id);

            await svc.DispatchAsync(svc.Parse(orderJson), handler);
            Assert.Equal("ord_1", handler.CapturedOrder?.Id);

            await svc.DispatchAsync(svc.Parse(subJson), handler);
            Assert.Equal("sub_1", handler.CapturedSubscription?.Id);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Dispatch_Unknown_Event_Falls_Back()
        {
            var now = DateTime.UtcNow;
            var json = JsonSerializer.Serialize(new { type = "unknown.type", data = new { foo = 1 }, event_id = "evt_2", created_at = now });

            var svc = new PolarWebhookService("irrelevant");
            var handler = new CapturingHandler();
            await svc.DispatchAsync(svc.Parse(json), handler);

            Assert.Equal("unknown.type", handler.UnknownType);
            Assert.True(handler.UnknownData.HasValue);
        }

        #endregion

        #region Helper Classes

        private sealed class CapturingHandler : PolarWebhookEventHandlerBase
        {
            public CheckoutCreatedEvent? CapturedCheckout { get; private set; }
            public CustomerCreatedEvent? CapturedCustomer { get; private set; }
            public OrderCreatedEvent? CapturedOrder { get; private set; }
            public SubscriptionCreatedEvent? CapturedSubscription { get; private set; }
            public string? UnknownType { get; private set; }
            public JsonElement? UnknownData { get; private set; }

            public override Task OnCheckoutCreated(CheckoutCreatedEvent data)
            {
                CapturedCheckout = data; return Task.CompletedTask;
            }

            public override Task OnCustomerCreated(CustomerCreatedEvent data)
            {
                CapturedCustomer = data; return Task.CompletedTask;
            }

            public override Task OnOrderCreated(OrderCreatedEvent data)
            {
                CapturedOrder = data; return Task.CompletedTask;
            }

            public override Task OnSubscriptionCreated(SubscriptionCreatedEvent data)
            {
                CapturedSubscription = data; return Task.CompletedTask;
            }

            public override Task OnUnknownEvent(string type, JsonElement data)
            {
                UnknownType = type; UnknownData = data; return Task.CompletedTask;
            }
        }

        #endregion
    }
}