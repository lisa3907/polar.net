using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PolarNet.Models;
using PolarNet.Services;
using Xunit;
using Xunit.Abstractions;

namespace PolarNet.Tests
{
    public class PolarRefundTests
    {
        private readonly ITestOutputHelper _output;
        private readonly PolarClient _client;
        private readonly bool _useSandbox;

        public PolarRefundTests(ITestOutputHelper output)
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

        [SkippableFact]
        public async Task ListRefunds_Should_Return_Paginated_Results()
        {
            Skip.IfNot(_useSandbox, "Integration tests only run in sandbox mode");

            // Act
            var result = await _client.ListRefundsAsync(page: 1, limit: 10);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Items);
            Assert.True(result.Items.Count >= 0);
            
            _output.WriteLine($"Found {result.Items.Count} refunds");
            
            if (result.Items.Count > 0)
            {
                var firstRefund = result.Items[0];
                Assert.NotNull(firstRefund.Id);
                Assert.True(firstRefund.Amount > 0);
                Assert.NotNull(firstRefund.Currency);
                Assert.NotNull(firstRefund.Reason);
                _output.WriteLine($"First refund: {firstRefund.Id}, Amount: {firstRefund.Amount} {firstRefund.Currency}, Reason: {firstRefund.Reason}");
            }
        }

        [SkippableFact]
        public async Task GetRefund_Should_Return_Refund_Details()
        {
            Skip.IfNot(_useSandbox, "Integration tests only run in sandbox mode");

            // Arrange - Get a refund ID first
            var refunds = await _client.ListRefundsAsync(page: 1, limit: 1);
            Skip.If(refunds.Items.Count == 0, "No refunds available for testing");
            
            var refundId = refunds.Items[0].Id;

            // Act
            var refund = await _client.GetRefundAsync(refundId);

            // Assert
            Assert.NotNull(refund);
            Assert.Equal(refundId, refund.Id);
            Assert.NotNull(refund.Status);
            Assert.NotNull(refund.Currency);
            Assert.NotNull(refund.Reason);
            Assert.True(refund.Amount > 0);
            
            _output.WriteLine($"Refund details - ID: {refund.Id}, Status: {refund.Status}, Amount: {refund.Amount} {refund.Currency}, Reason: {refund.Reason}");
        }

        [SkippableFact]
        public async Task Refund_Reason_Values_Should_Be_Valid()
        {
            Skip.IfNot(_useSandbox, "Integration tests only run in sandbox mode");

            // Act
            var refunds = await _client.ListRefundsAsync(page: 1, limit: 50);
            Skip.If(refunds.Items.Count == 0, "No refunds available for testing");

            // Assert - Check that all refund reasons are valid
            var validReasons = new[] { 
                "duplicate", 
                "fraudulent", 
                "requested_by_customer", 
                "expired_uncaptured_charge",
                "other"
            };
            
            foreach (var refund in refunds.Items)
            {
                Assert.NotNull(refund.Reason);
                Assert.Contains(refund.Reason, validReasons);
                _output.WriteLine($"Refund {refund.Id} has reason: {refund.Reason}");
            }
        }

        [SkippableFact]
        public async Task Refund_Status_Values_Should_Be_Valid()
        {
            Skip.IfNot(_useSandbox, "Integration tests only run in sandbox mode");

            // Act
            var refunds = await _client.ListRefundsAsync(page: 1, limit: 50);
            Skip.If(refunds.Items.Count == 0, "No refunds available for testing");

            // Assert - Check that all refund statuses are valid
            var validStatuses = new[] { "pending", "succeeded", "failed", "canceled" };
            
            foreach (var refund in refunds.Items)
            {
                Assert.NotNull(refund.Status);
                Assert.Contains(refund.Status, validStatuses);
                _output.WriteLine($"Refund {refund.Id} has status: {refund.Status}");
            }
        }

        [SkippableFact]
        public async Task Refund_CreatedAt_Should_Be_Valid_DateTime()
        {
            Skip.IfNot(_useSandbox, "Integration tests only run in sandbox mode");

            // Act
            var refunds = await _client.ListRefundsAsync(page: 1, limit: 10);
            Skip.If(refunds.Items.Count == 0, "No refunds available for testing");

            // Assert
            foreach (var refund in refunds.Items)
            {
                // CreatedAt is a value type (DateTimeOffset), no need for null check
                Assert.True(refund.CreatedAt <= DateTime.UtcNow);
                Assert.True(refund.CreatedAt > DateTime.UtcNow.AddYears(-5)); // Reasonable date range
                
                if (refund.ModifiedAt != null)
                {
                    Assert.True(refund.ModifiedAt >= refund.CreatedAt);
                }
            }
        }

        [SkippableFact]
        public Task CreateRefund_Request_Should_Have_Required_Fields()
        {
            // This is a unit test to verify the CreateRefundRequest model
            var request = new CreateRefundRequest
            {
                OrderId = "test-order-id",
                Reason = "requested_by_customer",
                Comment = "Customer requested refund"
            };

            Assert.NotNull(request.OrderId);
            Assert.NotNull(request.Reason);
            Assert.NotNull(request.Comment);
            
            // Verify valid reason values
            var validReasons = new[] { 
                "duplicate", 
                "fraudulent", 
                "requested_by_customer", 
                "expired_uncaptured_charge",
                "other"
            };
            Assert.Contains(request.Reason, validReasons);
            
            return Task.CompletedTask;
        }

        [SkippableFact]
        public Task CreatePartialRefund_Request_Should_Have_Amount()
        {
            // This is a unit test to verify partial refund request structure
            var request = new CreateRefundRequest
            {
                OrderId = "test-order-id",
                Amount = 1000, // Amount in cents
                Reason = "requested_by_customer",
                Comment = "Partial refund for damaged item"
            };

            Assert.NotNull(request.OrderId);
            Assert.NotNull(request.Amount);
            Assert.True(request.Amount > 0);
            Assert.NotNull(request.Reason);
            
            _output.WriteLine($"Partial refund request: Order {request.OrderId}, Amount: {request.Amount}");
            
            return Task.CompletedTask;
        }

        [SkippableFact]
        public async Task Refund_Should_Link_To_Order_And_Payment()
        {
            Skip.IfNot(_useSandbox, "Integration tests only run in sandbox mode");

            // Act
            var refunds = await _client.ListRefundsAsync(page: 1, limit: 10);
            Skip.If(refunds.Items.Count == 0, "No refunds available for testing");

            // Assert - Check that refunds have proper order and payment linkage
            foreach (var refund in refunds.Items)
            {
                Assert.NotNull(refund.OrderId);
                Assert.NotEmpty(refund.OrderId);
                
                // Payment ID might be optional depending on the refund type
                if (!string.IsNullOrEmpty(refund.PaymentId))
                {
                    _output.WriteLine($"Refund {refund.Id} linked to Order: {refund.OrderId}, Payment: {refund.PaymentId}");
                }
                else
                {
                    _output.WriteLine($"Refund {refund.Id} linked to Order: {refund.OrderId}");
                }
            }
        }
    }
}