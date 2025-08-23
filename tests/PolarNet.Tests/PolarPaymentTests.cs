using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PolarNet.Models;
using PolarNet.Services;
using Xunit;
using Xunit.Abstractions;

namespace PolarNet.Tests
{
    public class PolarPaymentTests
    {
        private readonly ITestOutputHelper _output;
        private readonly PolarClient _client;
        private readonly bool _useSandbox;

        public PolarPaymentTests(ITestOutputHelper output)
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
        public async Task ListPayments_Should_Return_Paginated_Results()
        {
            Skip.IfNot(_useSandbox, "Integration tests only run in sandbox mode");

            // Act
            var result = await _client.ListPaymentsAsync(page: 1, limit: 10);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Items);
            Assert.True(result.Items.Count >= 0);
            
            _output.WriteLine($"Found {result.Items.Count} payments");
            
            if (result.Items.Count > 0)
            {
                var firstPayment = result.Items[0];
                Assert.NotNull(firstPayment.Id);
                Assert.True(firstPayment.Amount > 0);
                Assert.NotNull(firstPayment.Currency);
                _output.WriteLine($"First payment: {firstPayment.Id}, Amount: {firstPayment.Amount} {firstPayment.Currency}");
            }
        }

        [SkippableFact]
        public async Task GetPayment_Should_Return_Payment_Details()
        {
            Skip.IfNot(_useSandbox, "Integration tests only run in sandbox mode");

            // Arrange - Get a payment ID first
            var payments = await _client.ListPaymentsAsync(page: 1, limit: 1);
            Skip.If(payments.Items.Count == 0, "No payments available for testing");
            
            var paymentId = payments.Items[0].Id;

            // Act
            var payment = await _client.GetPaymentAsync(paymentId);

            // Assert
            Assert.NotNull(payment);
            Assert.Equal(paymentId, payment.Id);
            Assert.NotNull(payment.Status);
            Assert.NotNull(payment.Currency);
            Assert.True(payment.Amount > 0);
            
            _output.WriteLine($"Payment details - ID: {payment.Id}, Status: {payment.Status}, Amount: {payment.Amount} {payment.Currency}");
        }

        [SkippableFact]
        public async Task ListPaymentsByOrder_Should_Return_Filtered_Results()
        {
            Skip.IfNot(_useSandbox, "Integration tests only run in sandbox mode");

            // Arrange - Find an order with payments
            var payments = await _client.ListPaymentsAsync(page: 1, limit: 10);
            var paymentWithOrder = payments.Items.Find(p => !string.IsNullOrEmpty(p.OrderId));
            Skip.If(paymentWithOrder == null, "No payments with orders available for testing");

            var orderId = paymentWithOrder.OrderId!;

            // Act
            var orderPayments = await _client.ListPaymentsByOrderAsync(orderId);

            // Assert
            Assert.NotNull(orderPayments);
            Assert.NotNull(orderPayments.Items);
            Assert.True(orderPayments.Items.Count > 0);
            Assert.All(orderPayments.Items, p => Assert.Equal(orderId, p.OrderId));
            
            _output.WriteLine($"Found {orderPayments.Items.Count} payments for order {orderId}");
        }

        // Customer-based filtering is not yet supported in the model
        // This test is commented out until CustomerId is added to PolarPayment model
        /*
        [SkippableFact]
        public async Task ListPaymentsByCustomer_Should_Return_Filtered_Results()
        {
            Skip.IfNot(_useSandbox, "Integration tests only run in sandbox mode");

            // This functionality requires CustomerId to be added to PolarPayment model
            Skip.If(true, "CustomerId property not yet implemented in PolarPayment model");
        }
        */

        [SkippableFact]
        public async Task Payment_Status_Values_Should_Be_Valid()
        {
            Skip.IfNot(_useSandbox, "Integration tests only run in sandbox mode");

            // Act
            var payments = await _client.ListPaymentsAsync(page: 1, limit: 50);
            Skip.If(payments.Items.Count == 0, "No payments available for testing");

            // Assert - Check that all payment statuses are valid
            var validStatuses = new[] { "pending", "succeeded", "failed", "canceled", "requires_action", "processing" };
            
            foreach (var payment in payments.Items)
            {
                Assert.NotNull(payment.Status);
                Assert.Contains(payment.Status, validStatuses);
                _output.WriteLine($"Payment {payment.Id} has status: {payment.Status}");
            }
        }

        [SkippableFact]
        public async Task Payment_CreatedAt_Should_Be_Valid_DateTime()
        {
            Skip.IfNot(_useSandbox, "Integration tests only run in sandbox mode");

            // Act
            var payments = await _client.ListPaymentsAsync(page: 1, limit: 10);
            Skip.If(payments.Items.Count == 0, "No payments available for testing");

            // Assert
            foreach (var payment in payments.Items)
            {
                Assert.NotNull(payment.CreatedAt);
                Assert.True(payment.CreatedAt <= DateTime.UtcNow);
                Assert.True(payment.CreatedAt > DateTime.UtcNow.AddYears(-5)); // Reasonable date range
                
                if (payment.UpdatedAt != null)
                {
                    Assert.True(payment.UpdatedAt >= payment.CreatedAt);
                }
            }
        }

        // Processor field is not yet supported in the model
        // This test is commented out until Processor is added to PolarPayment model
        /*
        [SkippableFact]
        public async Task Payment_Processor_Values_Should_Be_Valid()
        {
            Skip.IfNot(_useSandbox, "Integration tests only run in sandbox mode");

            // This functionality requires Processor to be added to PolarPayment model
            Skip.If(true, "Processor property not yet implemented in PolarPayment model");
        }
        */
    }
}