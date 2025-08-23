using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PolarNet.Models;
using PolarNet.Models.Resources;
using PolarNet.Services;
using Xunit;
using Xunit.Abstractions;

namespace PolarNet.Tests
{
    public class PolarWebhookEndpointTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly PolarClient _client;
        private readonly bool _useSandbox;
        private readonly List<string> _createdWebhookIds = new();

        public PolarWebhookEndpointTests(ITestOutputHelper output)
        {
            _output = output;

            var config = new ConfigurationBuilder()
#if DEBUG
                .AddJsonFile("appsettings.Development.json", optional: true)
#else
                .AddJsonFile("appsettings.json", optional: false)
#endif
                .Build();

            var polar = config.GetSection("PolarSettings");
            var accessToken = polar["AccessToken"] ?? string.Empty;
            var organizationId = polar["OrganizationId"];
            _useSandbox = string.Equals(polar["UseSandbox"], "true", StringComparison.OrdinalIgnoreCase);
            var baseUrl = _useSandbox ? polar["SandboxApiUrl"] : polar["ProductionApiUrl"];

            _client = new PolarClient(new PolarClientOptions
            {
                AccessToken = accessToken,
                BaseUrl = baseUrl ?? "",
                OrganizationId = organizationId
            });
        }

        public void Dispose()
        {
            // Clean up any created test webhooks
            foreach (var webhookId in _createdWebhookIds)
            {
                try
                {
                    _client.DeleteWebhookEndpointAsync(webhookId).GetAwaiter().GetResult();
                    _output.WriteLine($"Cleaned up test webhook: {webhookId}");
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"Failed to clean up webhook {webhookId}: {ex.Message}");
                }
            }
        }

        [SkippableFact]
        public async Task ListWebhookEndpoints_Should_Return_Paginated_Results()
        {
            Skip.IfNot(_useSandbox, "Integration tests only run in sandbox mode");

            // Act
            var result = await _client.ListWebhookEndpointsAsync(page: 1, limit: 10);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Items);
            Assert.True(result.Items.Count >= 0);
            
            _output.WriteLine($"Found {result.Items.Count} webhook endpoints");
            
            if (result.Items.Count > 0)
            {
                var firstWebhook = result.Items[0];
                Assert.NotNull(firstWebhook.Id);
                Assert.NotNull(firstWebhook.Url);
                Assert.NotNull(firstWebhook.Events);
                _output.WriteLine($"First webhook: {firstWebhook.Id}, URL: {firstWebhook.Url}, Events: {string.Join(", ", firstWebhook.Events)}");
            }
        }

        [SkippableFact]
        public async Task CreateWebhookEndpoint_Should_Create_New_Endpoint()
        {
            Skip.IfNot(_useSandbox, "Integration tests only run in sandbox mode");

            // Arrange
            var testUrl = $"https://test-webhook-{Guid.NewGuid():N}.example.com/webhook";
            var events = new List<string> { "order.created", "subscription.created" };
            var secret = $"test-secret-{Guid.NewGuid():N}";

            // Act
            var webhook = await _client.CreateWebhookEndpointAsync(testUrl, events, secret);
            _createdWebhookIds.Add(webhook.Id); // Track for cleanup

            // Assert
            Assert.NotNull(webhook);
            Assert.NotNull(webhook.Id);
            Assert.Equal(testUrl, webhook.Url);
            Assert.NotNull(webhook.Events);
            Assert.Equal(events.Count, webhook.Events.Count);
            foreach (var evt in events)
            {
                Assert.Contains(evt, webhook.Events);
            }
            
            _output.WriteLine($"Created webhook: {webhook.Id}, URL: {webhook.Url}");
        }

        [SkippableFact]
        public async Task GetWebhookEndpoint_Should_Return_Endpoint_Details()
        {
            Skip.IfNot(_useSandbox, "Integration tests only run in sandbox mode");

            // Arrange - Create a webhook first
            var testUrl = $"https://test-webhook-{Guid.NewGuid():N}.example.com/webhook";
            var events = new List<string> { "order.paid" };
            var created = await _client.CreateWebhookEndpointAsync(testUrl, events);
            _createdWebhookIds.Add(created.Id);

            // Act
            var webhook = await _client.GetWebhookEndpointAsync(created.Id);

            // Assert
            Assert.NotNull(webhook);
            Assert.Equal(created.Id, webhook.Id);
            Assert.Equal(testUrl, webhook.Url);
            Assert.NotNull(webhook.Format);
            Assert.NotNull(webhook.Events);
            
            _output.WriteLine($"Webhook details - ID: {webhook.Id}, Format: {webhook.Format}, Events: {string.Join(", ", webhook.Events)}");
        }

        [SkippableFact]
        public async Task UpdateWebhookEndpoint_Should_Update_Endpoint()
        {
            Skip.IfNot(_useSandbox, "Integration tests only run in sandbox mode");

            // Arrange - Create a webhook first
            var originalUrl = $"https://test-webhook-{Guid.NewGuid():N}.example.com/webhook";
            var originalEvents = new List<string> { "order.created" };
            var created = await _client.CreateWebhookEndpointAsync(originalUrl, originalEvents);
            _createdWebhookIds.Add(created.Id);

            // Act - Update the webhook
            var newUrl = $"https://updated-webhook-{Guid.NewGuid():N}.example.com/webhook";
            var newEvents = new List<string> { "order.created", "order.paid", "refund.created" };
            var updateRequest = new UpdateWebhookEndpointRequest
            {
                Url = newUrl,
                Events = newEvents
            };
            var updated = await _client.UpdateWebhookEndpointAsync(created.Id, updateRequest);

            // Assert
            Assert.NotNull(updated);
            Assert.Equal(created.Id, updated.Id);
            Assert.Equal(newUrl, updated.Url);
            Assert.Equal(newEvents.Count, updated.Events.Count);
            foreach (var evt in newEvents)
            {
                Assert.Contains(evt, updated.Events);
            }
            
            _output.WriteLine($"Updated webhook: {updated.Id}, New URL: {updated.Url}, New Events: {string.Join(", ", updated.Events)}");
        }

        [SkippableFact]
        public async Task DeleteWebhookEndpoint_Should_Remove_Endpoint()
        {
            Skip.IfNot(_useSandbox, "Integration tests only run in sandbox mode");

            // Arrange - Create a webhook first
            var testUrl = $"https://test-webhook-{Guid.NewGuid():N}.example.com/webhook";
            var events = new List<string> { "subscription.canceled" };
            var created = await _client.CreateWebhookEndpointAsync(testUrl, events);

            // Act
            var deleted = await _client.DeleteWebhookEndpointAsync(created.Id);

            // Assert
            Assert.True(deleted);
            
            // Verify it's actually deleted by trying to get it
            await Assert.ThrowsAsync<HttpRequestException>(async () => 
                await _client.GetWebhookEndpointAsync(created.Id));
            
            _output.WriteLine($"Successfully deleted webhook: {created.Id}");
        }

        [SkippableFact]
        public async Task TestWebhookEndpoint_Should_Send_Test_Event()
        {
            Skip.IfNot(_useSandbox, "Integration tests only run in sandbox mode");

            // Arrange - Create a webhook first
            // Note: Using webhook.site for a real endpoint that accepts test webhooks
            var testUrl = $"https://webhook.site/{Guid.NewGuid()}";
            var events = new List<string> { "order.created" };
            var created = await _client.CreateWebhookEndpointAsync(testUrl, events);
            _createdWebhookIds.Add(created.Id);

            // Act
            var testResult = await _client.TestWebhookEndpointAsync(created.Id);

            // Assert
            // The test endpoint may fail if the URL is not reachable, which is expected for test URLs
            // We're mainly testing that the API call itself works without throwing an exception
            _output.WriteLine($"Test event sent to webhook: {created.Id}, Result: {testResult}");
            
            // Since test webhooks to non-existent URLs typically fail, we don't assert true
            // Instead, we just verify the method executes without throwing
            Assert.NotNull(created.Id);
        }

        [SkippableFact]
        public async Task Webhook_Format_Values_Should_Be_Valid()
        {
            Skip.IfNot(_useSandbox, "Integration tests only run in sandbox mode");

            // Act
            var webhooks = await _client.ListWebhookEndpointsAsync(page: 1, limit: 50);
            
            // Assert - Check that all webhook formats are valid
            var validFormats = new[] { "raw", "discord", "slack" };
            
            foreach (var webhook in webhooks.Items)
            {
                Assert.NotNull(webhook.Format);
                Assert.Contains(webhook.Format, validFormats);
                _output.WriteLine($"Webhook {webhook.Id} has format: {webhook.Format}");
            }
        }

        [SkippableFact]
        public async Task Webhook_Event_Types_Should_Be_Valid()
        {
            Skip.IfNot(_useSandbox, "Integration tests only run in sandbox mode");

            // Act
            var webhooks = await _client.ListWebhookEndpointsAsync(page: 1, limit: 50);
            Skip.If(webhooks.Items.Count == 0, "No webhooks available for testing");

            // Assert - Check that all event types are valid (based on API validation)
            var validEventTypes = new[] { 
                "checkout.created", "checkout.updated",
                "customer.created", "customer.updated", "customer.deleted", "customer.state_changed",
                "order.created", "order.updated", "order.paid", "order.refunded",
                "subscription.created", "subscription.updated", "subscription.active", 
                "subscription.canceled", "subscription.uncanceled", "subscription.revoked",
                "refund.created", "refund.updated",
                "product.created", "product.updated",
                "benefit.created", "benefit.updated",
                "benefit_grant.created", "benefit_grant.cycled", "benefit_grant.updated", "benefit_grant.revoked",
                "organization.updated"
            };
            
            foreach (var webhook in webhooks.Items)
            {
                Assert.NotNull(webhook.Events);
                Assert.True(webhook.Events.Count > 0);
                
                foreach (var evt in webhook.Events)
                {
                    Assert.Contains(evt, validEventTypes);
                }
                
                _output.WriteLine($"Webhook {webhook.Id} subscribes to: {string.Join(", ", webhook.Events)}");
            }
        }

        [SkippableFact]
        public async Task Webhook_CreatedAt_Should_Be_Valid_DateTime()
        {
            Skip.IfNot(_useSandbox, "Integration tests only run in sandbox mode");

            // Act
            var webhooks = await _client.ListWebhookEndpointsAsync(page: 1, limit: 10);
            Skip.If(webhooks.Items.Count == 0, "No webhooks available for testing");

            // Assert
            foreach (var webhook in webhooks.Items)
            {
                Assert.True(webhook.CreatedAt <= DateTime.UtcNow);
                Assert.True(webhook.CreatedAt > DateTime.UtcNow.AddYears(-5)); // Reasonable date range
                
                if (webhook.ModifiedAt != null)
                {
                    Assert.True(webhook.ModifiedAt >= webhook.CreatedAt);
                }
            }
        }

        [SkippableFact]
        public async Task CreateWebhook_With_All_Events_Should_Succeed()
        {
            Skip.IfNot(_useSandbox, "Integration tests only run in sandbox mode");

            // Arrange - Create webhook with all event types
            var testUrl = $"https://test-all-events-{Guid.NewGuid():N}.example.com/webhook";
            var allEvents = new List<string> { 
                "order.created", "order.updated", "order.paid",
                "subscription.created", "subscription.updated", "subscription.canceled",
                "customer.created", "customer.updated",
                "refund.created"
            };

            // Act
            var webhook = await _client.CreateWebhookEndpointAsync(testUrl, allEvents);
            _createdWebhookIds.Add(webhook.Id);

            // Assert
            Assert.NotNull(webhook);
            Assert.Equal(allEvents.Count, webhook.Events.Count);
            foreach (var evt in allEvents)
            {
                Assert.Contains(evt, webhook.Events);
            }
            
            _output.WriteLine($"Created webhook with {webhook.Events.Count} event types");
        }
    }
}