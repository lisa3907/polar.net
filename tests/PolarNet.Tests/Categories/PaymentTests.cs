#if NETSTANDARD2_0 || NETSTANDARD2_1
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#else
#nullable enable
#endif

using System;
using System.Linq;
using System.Threading.Tasks;
using PolarNet.Tests.Base;
using Xunit;
using Xunit.Abstractions;

namespace PolarNet.Tests.Categories
{
    /// <summary>
    /// Tests for Payment API endpoints.
    /// </summary>
    public class PaymentTests : TestBase
    {
        public PaymentTests(PolarSandboxFixture fixture, ITestOutputHelper output) 
            : base(fixture, output)
        {
        }

        [Fact]
        public async Task ListPayments_ShouldReturnPayments()
        {
            // Arrange
            SkipIfNoClient(nameof(ListPayments_ShouldReturnPayments));
            LogSection("List Payments Test");

            // Act
            Log("Fetching payments...");
            var payments = await Client!.ListPaymentsAsync(1, 10);

            // Assert
            Assert.NotNull(payments);
            Assert.NotNull(payments.Items);
            
            if (payments.Items.Count > 0)
            {
                Log($"✓ Found {payments.Items.Count} payments");
                
                foreach (var payment in payments.Items.Take(3))
                {
                    Assert.NotEmpty(payment.Id);
                    Assert.NotNull(payment.Status);
                    Assert.NotEmpty(payment.Status);
                    Assert.True(payment.Amount > 0);
                    Assert.NotNull(payment.Currency);
                    Assert.NotEmpty(payment.Currency);
                    
                    Log($"  - Payment ID: {payment.Id}");
                    Log($"    Status: {payment.Status}");
                    Log($"    Amount: {payment.Amount} {payment.Currency}");
                    Log($"    Created: {payment.CreatedAt:yyyy-MM-dd HH:mm:ss}");
                    
                    // Note: CustomerId field is not available in PolarPayment model
                    
                    if (!string.IsNullOrEmpty(payment.OrderId))
                    {
                        Log($"    Order: {payment.OrderId}");
                    }
                }
            }
            else
            {
                Log("⚠ No payments found (this may be expected for a new organization)");
            }
        }

        [Fact]
        public async Task GetPayment_WithValidId_ShouldReturnPaymentDetails()
        {
            // Arrange
            SkipIfNoClient(nameof(GetPayment_WithValidId_ShouldReturnPaymentDetails));
            
            // First, get any existing payment
            var payments = await Client!.ListPaymentsAsync(1, 1);
            Skip.If(payments.Items.Count == 0, "No payments available to test");
            
            var paymentId = payments.Items[0].Id;
            LogSection("Get Payment Test");

            // Act
            Log($"Fetching payment {paymentId}...");
            var payment = await Client.GetPaymentAsync(paymentId);

            // Assert
            Assert.NotNull(payment);
            Assert.Equal(paymentId, payment.Id);
            Assert.NotNull(payment.Status);
            Assert.NotEmpty(payment.Status);
            Assert.True(payment.Amount > 0);
            Assert.NotNull(payment.Currency);
            Assert.NotEmpty(payment.Currency);
            Assert.True(payment.CreatedAt > DateTime.MinValue);
            
            Log($"✓ Payment retrieved: {payment.Id}");
            Log($"  Status: {payment.Status}");
            Log($"  Amount: {payment.Amount} {payment.Currency}");
            // Note: CustomerId field is not available in PolarPayment model
            Log($"  Order ID: {payment.OrderId ?? "N/A"}");
            // Note: SubscriptionId field is not available in PolarPayment model
            Log($"  Created: {payment.CreatedAt:yyyy-MM-dd HH:mm:ss}");
        }

        [Fact]
        public async Task ListPaymentsByOrder_WithValidOrderId_ShouldReturnPayments()
        {
            // Arrange
            SkipIfNoClient(nameof(ListPaymentsByOrder_WithValidOrderId_ShouldReturnPayments));
            
            // First, get an order that has payments
            var orders = await Client!.ListOrdersAsync(1, 10);
            Skip.If(orders.Items.Count == 0, "No orders available to test");
            
            string? orderWithPayment = null;
            foreach (var order in orders.Items)
            {
                var payments = await Client.ListPaymentsByOrderAsync(order.Id, 1, 1);
                if (payments.Items.Count > 0)
                {
                    orderWithPayment = order.Id;
                    break;
                }
            }
            
            Skip.If(orderWithPayment == null, "No orders with payments found");
            LogSection("List Payments by Order Test");

            // Act
            Log($"Fetching payments for order {orderWithPayment}...");
            var orderPayments = await Client.ListPaymentsByOrderAsync(orderWithPayment!, 1, 10);

            // Assert
            Assert.NotNull(orderPayments);
            Assert.NotNull(orderPayments.Items);
            Assert.True(orderPayments.Items.Count > 0);
            
            foreach (var payment in orderPayments.Items)
            {
                Assert.Equal(orderWithPayment, payment.OrderId);
            }
            
            Log($"✓ Found {orderPayments.Items.Count} payments for order");
        }

        [Fact(Skip = "CustomerId field not available in PolarPayment model")]
        public async Task ListPaymentsByCustomer_WithValidCustomerId_ShouldReturnPayments()
        {
            // This test is skipped because CustomerId field is not available in the current PolarPayment model
            // It should be re-enabled once the model is updated to include CustomerId
            
            // Arrange
            SkipIfNoClient(nameof(ListPaymentsByCustomer_WithValidCustomerId_ShouldReturnPayments));
            
            // Test implementation will be added when CustomerId is available in the model
            await Task.CompletedTask;
        }

        [Fact]
        public async Task ListPayments_WithPagination_ShouldRespectLimits()
        {
            // Arrange
            SkipIfNoClient(nameof(ListPayments_WithPagination_ShouldRespectLimits));
            const int requestedLimit = 5;
            
            // Act
            var payments = await Client!.ListPaymentsAsync(1, requestedLimit);

            // Assert
            Assert.NotNull(payments);
            Assert.NotNull(payments.Items);
            Assert.True(payments.Items.Count <= requestedLimit, 
                $"Expected at most {requestedLimit} items, but got {payments.Items.Count}");
            
            Log($"✓ Pagination respected: requested {requestedLimit}, received {payments.Items.Count}");
        }

        [Fact]
        public async Task GetPayment_ShouldHaveValidMonetaryValues()
        {
            // Arrange
            SkipIfNoClient(nameof(GetPayment_ShouldHaveValidMonetaryValues));
            
            var payments = await Client!.ListPaymentsAsync(1, 1);
            Skip.If(payments.Items.Count == 0, "No payments available to test");
            
            var paymentId = payments.Items[0].Id;

            // Act
            var payment = await Client.GetPaymentAsync(paymentId);

            // Assert
            Assert.NotNull(payment);
            Assert.True(payment.Amount > 0, "Amount should be positive");
            Assert.NotNull(payment.Currency);
            Assert.NotEmpty(payment.Currency);
            Assert.Matches("^[A-Z]{3}$", payment.Currency); // Should be 3-letter currency code
            
            Log($"✓ Payment monetary values valid");
            Log($"  Amount: {payment.Amount}");
            Log($"  Currency: {payment.Currency}");
        }

        [Fact]
        public async Task GetPayment_ShouldHaveValidStatus()
        {
            // Arrange
            SkipIfNoClient(nameof(GetPayment_ShouldHaveValidStatus));
            
            var payments = await Client!.ListPaymentsAsync(1, 1);
            Skip.If(payments.Items.Count == 0, "No payments available to test");
            
            var paymentId = payments.Items[0].Id;

            // Act
            var payment = await Client.GetPaymentAsync(paymentId);

            // Assert
            Assert.NotNull(payment);
            var validStatuses = new[] { "pending", "succeeded", "failed", "processing", "canceled" };
            Assert.Contains(payment.Status, validStatuses);
            
            Log($"✓ Payment status valid: {payment.Status}");
        }
    }
}