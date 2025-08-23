using System;
using System.Linq;
using System.Threading.Tasks;
using PolarNet.Models;
using PolarNet.Tests.Base;
using Xunit;
using Xunit.Abstractions;

namespace PolarNet.Tests.Categories
{
    /// <summary>
    /// Tests for Refund API endpoints.
    /// </summary>
    public class RefundTests : TestBase
    {
        public RefundTests(PolarSandboxFixture fixture, ITestOutputHelper output) 
            : base(fixture, output)
        {
        }

        [Fact]
        public async Task ListRefunds_ShouldReturnRefunds()
        {
            // Arrange
            SkipIfNoClient(nameof(ListRefunds_ShouldReturnRefunds));
            LogSection("List Refunds Test");

            // Act
            Log("Fetching refunds...");
            var refunds = await Client!.ListRefundsAsync(1, 10);

            // Assert
            Assert.NotNull(refunds);
            Assert.NotNull(refunds.Items);
            
            if (refunds.Items.Count > 0)
            {
                Log($"✓ Found {refunds.Items.Count} refunds");
                
                foreach (var refund in refunds.Items.Take(3))
                {
                    Assert.NotEmpty(refund.Id);
                    Assert.NotEmpty(refund.Status);
                    Assert.NotEmpty(refund.Reason);
                    Assert.True(refund.Amount > 0);
                    Assert.NotEmpty(refund.Currency);
                    
                    Log($"  - Refund ID: {refund.Id}");
                    Log($"    Status: {refund.Status}");
                    Log($"    Amount: {refund.Amount} {refund.Currency}");
                    Log($"    Reason: {refund.Reason}");
                    Log($"    Created: {refund.CreatedAt:yyyy-MM-dd HH:mm:ss}");
                    
                    if (!string.IsNullOrEmpty(refund.Comment))
                    {
                        Log($"    Comment: {refund.Comment}");
                    }
                }
            }
            else
            {
                Log("⚠ No refunds found (this is normal if no refunds have been processed)");
            }
        }

        [Fact]
        public async Task GetRefund_WithValidId_ShouldReturnRefundDetails()
        {
            // Arrange
            SkipIfNoClient(nameof(GetRefund_WithValidId_ShouldReturnRefundDetails));
            
            // First, get any existing refund
            var refunds = await Client!.ListRefundsAsync(1, 1);
            Skip.If(refunds.Items.Count == 0, "No refunds available to test");
            
            var refundId = refunds.Items[0].Id;
            LogSection("Get Refund Test");

            // Act
            Log($"Fetching refund {refundId}...");
            var refund = await Client.GetRefundAsync(refundId);

            // Assert
            Assert.NotNull(refund);
            Assert.Equal(refundId, refund.Id);
            Assert.NotEmpty(refund.Status);
            Assert.NotEmpty(refund.Reason);
            Assert.True(refund.Amount > 0);
            Assert.NotEmpty(refund.Currency);
            Assert.True(refund.CreatedAt > DateTime.MinValue);
            
            Log($"✓ Refund retrieved: {refund.Id}");
            Log($"  Status: {refund.Status}");
            Log($"  Amount: {refund.Amount} {refund.Currency}");
            Log($"  Reason: {refund.Reason}");
            Log($"  Comment: {refund.Comment ?? "N/A"}");
            Log($"  Created: {refund.CreatedAt:yyyy-MM-dd HH:mm:ss}");
        }

        [Fact(Skip = "Requires a valid paid order to test refund creation")]
        public async Task CreateRefund_WithValidOrder_ShouldSucceed()
        {
            // This test is skipped by default as it requires a real paid order
            // Uncomment and modify for integration testing with actual paid orders
            
            // Arrange
            SkipIfNoClient(nameof(CreateRefund_WithValidOrder_ShouldSucceed));
            const string orderId = "test_order_id"; // Replace with actual order ID
            const string reason = "customer_request";
            const string comment = "Test refund";

            // Act
            var refund = await Client!.CreateRefundAsync(orderId, reason, comment);

            // Assert
            Assert.NotNull(refund);
            Assert.NotEmpty(refund.Id);
            Assert.Equal(reason, refund.Reason);
            Assert.Equal(comment, refund.Comment);
        }

        [Fact(Skip = "Requires a valid paid order to test partial refund creation")]
        public async Task CreatePartialRefund_WithValidOrder_ShouldSucceed()
        {
            // This test is skipped by default as it requires a real paid order
            // Uncomment and modify for integration testing with actual paid orders
            
            // Arrange
            SkipIfNoClient(nameof(CreatePartialRefund_WithValidOrder_ShouldSucceed));
            const string orderId = "test_order_id"; // Replace with actual order ID
            const long amount = 500; // Amount in minor units (e.g., cents)
            const string reason = "customer_request";
            const string comment = "Partial refund test";

            // Act
            var refund = await Client!.CreatePartialRefundAsync(orderId, amount, reason, comment);

            // Assert
            Assert.NotNull(refund);
            Assert.NotEmpty(refund.Id);
            Assert.Equal(amount, refund.Amount);
            Assert.Equal(reason, refund.Reason);
            Assert.Equal(comment, refund.Comment);
        }

        [Fact(Skip = "Requires a valid paid order to test refund creation with request object")]
        public async Task CreateRefund_WithRequest_ShouldSucceed()
        {
            // This test is skipped by default as it requires a real paid order
            // Uncomment and modify for integration testing with actual paid orders
            
            // Arrange
            SkipIfNoClient(nameof(CreateRefund_WithRequest_ShouldSucceed));
            var request = new CreateRefundRequest
            {
                OrderId = "test_order_id", // Replace with actual order ID
                Reason = "duplicate",
                Comment = "Duplicate order - refunding",
                Amount = 1000 // Optional: for partial refund
            };

            // Act
            var refund = await Client!.CreateRefundAsync(request);

            // Assert
            Assert.NotNull(refund);
            Assert.NotEmpty(refund.Id);
            Assert.Equal(request.Reason, refund.Reason);
            Assert.Equal(request.Comment, refund.Comment);
            if (request.Amount.HasValue)
            {
                Assert.Equal(request.Amount.Value, refund.Amount);
            }
        }

        [Fact]
        public async Task ListRefunds_WithPagination_ShouldRespectLimits()
        {
            // Arrange
            SkipIfNoClient(nameof(ListRefunds_WithPagination_ShouldRespectLimits));
            const int requestedLimit = 5;
            
            // Act
            var refunds = await Client!.ListRefundsAsync(1, requestedLimit);

            // Assert
            Assert.NotNull(refunds);
            Assert.NotNull(refunds.Items);
            Assert.True(refunds.Items.Count <= requestedLimit, 
                $"Expected at most {requestedLimit} items, but got {refunds.Items.Count}");
            
            Log($"✓ Pagination respected: requested {requestedLimit}, received {refunds.Items.Count}");
        }

        [Fact]
        public async Task GetRefund_ShouldHaveValidStatus()
        {
            // Arrange
            SkipIfNoClient(nameof(GetRefund_ShouldHaveValidStatus));
            
            var refunds = await Client!.ListRefundsAsync(1, 1);
            Skip.If(refunds.Items.Count == 0, "No refunds available to test");
            
            var refundId = refunds.Items[0].Id;

            // Act
            var refund = await Client.GetRefundAsync(refundId);

            // Assert
            Assert.NotNull(refund);
            var validStatuses = new[] { "pending", "succeeded", "failed", "canceled" };
            Assert.Contains(refund.Status, validStatuses);
            
            Log($"✓ Refund status valid: {refund.Status}");
        }

        [Fact]
        public async Task GetRefund_ShouldHaveValidReason()
        {
            // Arrange
            SkipIfNoClient(nameof(GetRefund_ShouldHaveValidReason));
            
            var refunds = await Client!.ListRefundsAsync(1, 1);
            Skip.If(refunds.Items.Count == 0, "No refunds available to test");
            
            var refundId = refunds.Items[0].Id;

            // Act
            var refund = await Client.GetRefundAsync(refundId);

            // Assert
            Assert.NotNull(refund);
            var validReasons = new[] { "duplicate", "fraudulent", "customer_request", "other" };
            Assert.Contains(refund.Reason, validReasons);
            
            Log($"✓ Refund reason valid: {refund.Reason}");
        }
    }
}