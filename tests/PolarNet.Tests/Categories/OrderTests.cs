using System;
using System.Linq;
using System.Threading.Tasks;
using PolarNet.Tests.Base;
using Xunit;
using Xunit.Abstractions;

namespace PolarNet.Tests.Categories
{
    /// <summary>
    /// Tests for Order API endpoints.
    /// </summary>
    public class OrderTests : TestBase
    {
        public OrderTests(PolarSandboxFixture fixture, ITestOutputHelper output) 
            : base(fixture, output)
        {
        }

        [Fact]
        public async Task ListOrders_ShouldReturnOrders()
        {
            // Arrange
            SkipIfNoClient(nameof(ListOrders_ShouldReturnOrders));
            LogSection("List Orders Test");

            // Act
            Log("Fetching orders...");
            var orders = await Client!.ListOrdersAsync(1, 10);

            // Assert
            Assert.NotNull(orders);
            Assert.NotNull(orders.Items);
            
            if (orders.Items.Count > 0)
            {
                Log($"✓ Found {orders.Items.Count} orders");
                
                foreach (var order in orders.Items.Take(3))
                {
                    Assert.NotEmpty(order.Id);
                    Assert.NotEmpty(order.CustomerId);
                    Assert.True(order.Amount >= 0);
                    Assert.NotEmpty(order.Currency);
                    
                    Log($"  - Order ID: {order.Id}");
                    Log($"    Customer: {order.CustomerId}");
                    Log($"    Amount: {order.Amount} {order.Currency}");
                    Log($"    Created: {order.CreatedAt:yyyy-MM-dd HH:mm:ss}");
                    
                    if (!string.IsNullOrEmpty(order.ProductId))
                    {
                        Log($"    Product: {order.ProductId}");
                    }
                    
                    if (!string.IsNullOrEmpty(order.SubscriptionId))
                    {
                        Log($"    Subscription: {order.SubscriptionId}");
                    }
                }
            }
            else
            {
                Log("⚠ No orders found (this may be expected for a new organization)");
            }
        }

        [Fact]
        public async Task GetOrder_WithValidId_ShouldReturnOrderDetails()
        {
            // Arrange
            SkipIfNoClient(nameof(GetOrder_WithValidId_ShouldReturnOrderDetails));
            
            // First, get any existing order
            var orders = await Client!.ListOrdersAsync(1, 1);
            Skip.If(orders.Items.Count == 0, "No orders available to test");
            
            var orderId = orders.Items[0].Id;
            LogSection("Get Order Test");

            // Act
            Log($"Fetching order {orderId}...");
            var order = await Client.GetOrderAsync(orderId);

            // Assert
            Assert.NotNull(order);
            Assert.Equal(orderId, order.Id);
            Assert.NotEmpty(order.CustomerId);
            Assert.True(order.Amount >= 0);
            Assert.NotEmpty(order.Currency);
            Assert.True(order.CreatedAt > DateTime.MinValue);
            
            Log($"✓ Order retrieved: {order.Id}");
            Log($"  Customer ID: {order.CustomerId}");
            Log($"  Amount: {order.Amount} {order.Currency}");
            Log($"  Product ID: {order.ProductId ?? "N/A"}");
            Log($"  Product Price ID: {order.ProductPriceId ?? "N/A"}");
            Log($"  Subscription ID: {order.SubscriptionId ?? "N/A"}");
            Log($"  Created: {order.CreatedAt:yyyy-MM-dd HH:mm:ss}");
        }

        [Fact]
        public async Task ListOrders_WithPagination_ShouldRespectLimits()
        {
            // Arrange
            SkipIfNoClient(nameof(ListOrders_WithPagination_ShouldRespectLimits));
            const int requestedLimit = 5;
            
            // Act
            var orders = await Client!.ListOrdersAsync(1, requestedLimit);

            // Assert
            Assert.NotNull(orders);
            Assert.NotNull(orders.Items);
            Assert.True(orders.Items.Count <= requestedLimit, 
                $"Expected at most {requestedLimit} items, but got {orders.Items.Count}");
            
            Log($"✓ Pagination respected: requested {requestedLimit}, received {orders.Items.Count}");
        }

        [Fact]
        public async Task GetOrder_ShouldHaveValidTimestamps()
        {
            // Arrange
            SkipIfNoClient(nameof(GetOrder_ShouldHaveValidTimestamps));
            
            var orders = await Client!.ListOrdersAsync(1, 1);
            Skip.If(orders.Items.Count == 0, "No orders available to test");
            
            var orderId = orders.Items[0].Id;

            // Act
            var order = await Client.GetOrderAsync(orderId);

            // Assert
            Assert.NotNull(order);
            Assert.True(order.CreatedAt > DateTime.MinValue);
            Assert.True(order.CreatedAt <= DateTime.UtcNow);
            
            Log($"✓ Order timestamps valid");
            Log($"  Created: {order.CreatedAt:O}");
        }

        [Fact]
        public async Task GetOrder_ShouldHaveValidMonetaryValues()
        {
            // Arrange
            SkipIfNoClient(nameof(GetOrder_ShouldHaveValidMonetaryValues));
            
            var orders = await Client!.ListOrdersAsync(1, 1);
            Skip.If(orders.Items.Count == 0, "No orders available to test");
            
            var orderId = orders.Items[0].Id;

            // Act
            var order = await Client.GetOrderAsync(orderId);

            // Assert
            Assert.NotNull(order);
            Assert.True(order.Amount >= 0, "Amount should be non-negative");
            Assert.NotEmpty(order.Currency);
            Assert.Matches("^[A-Z]{3}$", order.Currency); // Should be 3-letter currency code
            
            Log($"✓ Order monetary values valid");
            Log($"  Amount: {order.Amount}");
            Log($"  Currency: {order.Currency}");
        }
    }
}