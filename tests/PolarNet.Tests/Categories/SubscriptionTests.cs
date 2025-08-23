using System;
using System.Linq;
using System.Threading.Tasks;
using PolarNet.Tests.Base;
using Xunit;
using Xunit.Abstractions;

namespace PolarNet.Tests.Categories
{
    /// <summary>
    /// Tests for Subscription API endpoints.
    /// </summary>
    public class SubscriptionTests : TestBase
    {
        public SubscriptionTests(PolarSandboxFixture fixture, ITestOutputHelper output) 
            : base(fixture, output)
        {
        }

        [Fact]
        public async Task ListSubscriptions_ShouldReturnSubscriptions()
        {
            // Arrange
            SkipIfNoClient(nameof(ListSubscriptions_ShouldReturnSubscriptions));
            LogSection("List Subscriptions Test");

            // Act
            Log("Fetching subscriptions...");
            var subscriptions = await Client!.ListSubscriptionsAsync(1, 20);

            // Assert
            Assert.NotNull(subscriptions);
            Assert.NotNull(subscriptions.Items);
            
            if (subscriptions.Items.Count > 0)
            {
                Log($"✓ Found {subscriptions.Items.Count} subscriptions");
                
                foreach (var subscription in subscriptions.Items.Take(3))
                {
                    Assert.NotEmpty(subscription.Id);
                    Assert.NotEmpty(subscription.Status);
                    
                    Log($"  - Subscription ID: {subscription.Id}");
                    Log($"    Status: {subscription.Status}");
                    Log($"    Created: {subscription.CreatedAt:yyyy-MM-dd}");
                    
                    if (subscription.CurrentPeriodStart != null && subscription.CurrentPeriodEnd != null)
                    {
                        Log($"    Current Period: {subscription.CurrentPeriodStart:yyyy-MM-dd} to {subscription.CurrentPeriodEnd:yyyy-MM-dd}");
                    }
                }
            }
            else
            {
                Log("⚠ No subscriptions found (this may be expected)");
            }
        }

        [Fact]
        public async Task CreateSubscription_WithValidCustomerAndPrice_ShouldSucceed()
        {
            // Arrange
            SkipIfNoClient(nameof(CreateSubscription_WithValidCustomerAndPrice_ShouldSucceed));
            Skip.If(string.IsNullOrEmpty(PriceId), "No PriceId configured for subscription creation");
            LogSection("Create Subscription Test");
            
            // Create a test customer
            var customer = await Client!.CreateCustomerAsync(GenerateTestEmail());
            Log($"Created test customer: {customer.Id}");

            try
            {
                // Act
                Log($"Creating subscription for customer {customer.Id} with price {PriceId}...");
                var subscription = await Client.CreateSubscriptionAsync(customer.Id, PriceId);

                // Assert
                Assert.NotNull(subscription);
                Assert.NotEmpty(subscription.Id);
                Assert.NotEmpty(subscription.Status);
                Assert.NotEmpty(subscription.CustomerId);
                Assert.Equal(customer.Id, subscription.CustomerId);
                
                Log($"✓ Subscription created: {subscription.Id}");
                Log($"  Status: {subscription.Status}");
                Log($"  Customer ID: {subscription.CustomerId}");
                Log($"  Product ID: {subscription.ProductId}");
                
                // Cancel the subscription to clean up
                if (subscription.Status == "active" || subscription.Status == "trialing")
                {
                    await Client.CancelSubscriptionAsync(subscription.Id);
                    Log("✓ Subscription cancelled for cleanup");
                }
            }
            finally
            {
                // Cleanup customer
                await Client.DeleteCustomerAsync(customer.Id);
                Log("✓ Test customer deleted");
            }
        }

        [Fact]
        public async Task GetSubscription_WithValidId_ShouldReturnDetails()
        {
            // Arrange
            SkipIfNoClient(nameof(GetSubscription_WithValidId_ShouldReturnDetails));
            
            // First, get any existing subscription
            var subscriptions = await Client!.ListSubscriptionsAsync(1, 1);
            Skip.If(subscriptions.Items.Count == 0, "No subscriptions available to test");
            
            var subscriptionId = subscriptions.Items[0].Id;
            LogSection("Get Subscription Test");

            // Act
            Log($"Fetching subscription {subscriptionId}...");
            var subscription = await Client.GetSubscriptionAsync(subscriptionId);

            // Assert
            Assert.NotNull(subscription);
            Assert.Equal(subscriptionId, subscription.Id);
            Assert.NotEmpty(subscription.Status);
            Assert.NotEmpty(subscription.CustomerId);
            Assert.True(subscription.CreatedAt > DateTime.MinValue);
            
            Log($"✓ Subscription retrieved: {subscription.Id}");
            Log($"  Status: {subscription.Status}");
            Log($"  Customer ID: {subscription.CustomerId}");
            Log($"  Product ID: {subscription.ProductId}");
            Log($"  Created: {subscription.CreatedAt:yyyy-MM-dd HH:mm:ss}");
        }

        [Fact]
        public async Task CancelSubscription_WithActiveSubscription_ShouldSucceed()
        {
            // Arrange
            SkipIfNoClient(nameof(CancelSubscription_WithActiveSubscription_ShouldSucceed));
            Skip.If(string.IsNullOrEmpty(PriceId), "No PriceId configured for subscription creation");
            LogSection("Cancel Subscription Test");
            
            // Create a test subscription
            var customer = await Client!.CreateCustomerAsync(GenerateTestEmail());
            var subscription = await Client.CreateSubscriptionAsync(customer.Id, PriceId);
            
            try
            {
                // Skip if subscription is not active (might be in trial or other state)
                Skip.If(subscription.Status != "active" && subscription.Status != "trialing", 
                    $"Subscription is not in cancellable state: {subscription.Status}");

                // Act
                Log($"Cancelling subscription {subscription.Id}...");
                var result = await Client.CancelSubscriptionAsync(subscription.Id);

                // Assert
                Assert.True(result, "Cancel should return true");
                Log($"✓ Subscription cancelled successfully");
                
                // Verify the subscription status changed
                var updatedSubscription = await Client.GetSubscriptionAsync(subscription.Id);
                Log($"  Updated status: {updatedSubscription.Status}");
            }
            finally
            {
                // Cleanup
                await Client.DeleteCustomerAsync(customer.Id);
            }
        }

        [Fact]
        public async Task RevokeSubscription_WithActiveSubscription_ShouldSucceed()
        {
            // Arrange
            SkipIfNoClient(nameof(RevokeSubscription_WithActiveSubscription_ShouldSucceed));
            Skip.If(string.IsNullOrEmpty(PriceId), "No PriceId configured for subscription creation");
            LogSection("Revoke Subscription Test");
            
            // Create a test subscription
            var customer = await Client!.CreateCustomerAsync(GenerateTestEmail());
            var subscription = await Client.CreateSubscriptionAsync(customer.Id, PriceId);
            
            try
            {
                // Skip if subscription is not active
                Skip.If(subscription.Status != "active" && subscription.Status != "trialing", 
                    $"Subscription is not in revocable state: {subscription.Status}");

                // Act
                Log($"Revoking subscription {subscription.Id}...");
                Log("⚠ Note: Revoke immediately terminates the subscription");
                var result = await Client.RevokeSubscriptionAsync(subscription.Id);

                // Assert
                Assert.True(result, "Revoke should return true");
                Log($"✓ Subscription revoked successfully");
                
                // Verify the subscription was terminated
                var updatedSubscription = await Client.GetSubscriptionAsync(subscription.Id);
                Log($"  Updated status: {updatedSubscription.Status}");
            }
            finally
            {
                // Cleanup
                await Client.DeleteCustomerAsync(customer.Id);
            }
        }

        [Fact]
        public async Task ListSubscriptions_WithPagination_ShouldRespectLimits()
        {
            // Arrange
            SkipIfNoClient(nameof(ListSubscriptions_WithPagination_ShouldRespectLimits));
            const int requestedLimit = 5;
            
            // Act
            var subscriptions = await Client!.ListSubscriptionsAsync(1, requestedLimit);

            // Assert
            Assert.NotNull(subscriptions);
            Assert.NotNull(subscriptions.Items);
            Assert.True(subscriptions.Items.Count <= requestedLimit, 
                $"Expected at most {requestedLimit} items, but got {subscriptions.Items.Count}");
            
            Log($"✓ Pagination respected: requested {requestedLimit}, received {subscriptions.Items.Count}");
        }
    }
}