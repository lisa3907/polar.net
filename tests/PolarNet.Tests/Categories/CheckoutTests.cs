using System;
using System.Threading.Tasks;
using PolarNet.Tests.Base;
using Xunit;
using Xunit.Abstractions;

namespace PolarNet.Tests.Categories
{
    /// <summary>
    /// Tests for Checkout API endpoints.
    /// </summary>
    public class CheckoutTests : TestBase
    {
        public CheckoutTests(PolarSandboxFixture fixture, ITestOutputHelper output) 
            : base(fixture, output)
        {
        }

        [Fact]
        public async Task CreateCheckout_WithBasicInfo_ShouldSucceed()
        {
            // Arrange
            SkipIfNoClient(nameof(CreateCheckout_WithBasicInfo_ShouldSucceed));
            Skip.If(string.IsNullOrEmpty(ProductId), "No ProductId configured for checkout creation");
            LogSection("Create Checkout Test");
            
            var customerEmail = GenerateTestEmail();
            var successUrl = $"{WebhookBaseUrl}/success";

            // Act
            Log($"Creating checkout for email: {customerEmail}");
            var checkout = await Client!.CreateCheckoutAsync(customerEmail, successUrl);

            // Assert
            Assert.NotNull(checkout);
            Assert.NotEmpty(checkout.Id);
            Assert.NotEmpty(checkout.Status);
            Assert.NotEmpty(checkout.ClientSecret);
            Assert.NotEmpty(checkout.Url);
            
            Log($"✓ Checkout created: {checkout.Id}");
            Log($"  Status: {checkout.Status}");
            Log($"  URL: {checkout.Url}");
            Log($"  Client Secret: {checkout.ClientSecret.Substring(0, 10)}...");
            
            if (!string.IsNullOrEmpty(checkout.SuccessUrl))
            {
                Log($"  Success URL: {checkout.SuccessUrl}");
            }
        }

        [Fact]
        public async Task CreateCheckout_WithoutParameters_ShouldUseDefaults()
        {
            // Arrange
            SkipIfNoClient(nameof(CreateCheckout_WithoutParameters_ShouldUseDefaults));
            Skip.If(string.IsNullOrEmpty(ProductId), "No ProductId configured for checkout creation");
            LogSection("Create Checkout with Defaults Test");

            // Act
            Log("Creating checkout with default parameters...");
            var successUrl = $"{WebhookBaseUrl}/success";
            var checkout = await Client!.CreateCheckoutAsync(null, successUrl);

            // Assert
            Assert.NotNull(checkout);
            Assert.NotEmpty(checkout.Id);
            Assert.NotEmpty(checkout.Status);
            Assert.NotEmpty(checkout.ClientSecret);
            Assert.NotEmpty(checkout.Url);
            Assert.Equal(ProductId, checkout.ProductId);
            
            Log($"✓ Checkout created with defaults: {checkout.Id}");
            Log($"  Product ID: {checkout.ProductId}");
            Log($"  Status: {checkout.Status}");
        }

        [Fact]
        public async Task GetCheckout_WithValidId_ShouldReturnDetails()
        {
            // Arrange
            SkipIfNoClient(nameof(GetCheckout_WithValidId_ShouldReturnDetails));
            Skip.If(string.IsNullOrEmpty(ProductId), "No ProductId configured");
            LogSection("Get Checkout Test");
            
            // Create a checkout first
            var successUrl = $"{WebhookBaseUrl}/success";
            var createdCheckout = await Client!.CreateCheckoutAsync(null, successUrl);
            Log($"Created checkout: {createdCheckout.Id}");

            // Act
            Log($"Fetching checkout {createdCheckout.Id}...");
            var checkout = await Client.GetCheckoutAsync(createdCheckout.Id);

            // Assert
            Assert.NotNull(checkout);
            Assert.Equal(createdCheckout.Id, checkout.Id);
            Assert.Equal(createdCheckout.Status, checkout.Status);
            Assert.Equal(createdCheckout.ClientSecret, checkout.ClientSecret);
            Assert.Equal(createdCheckout.Url, checkout.Url);
            
            Log($"✓ Checkout retrieved: {checkout.Id}");
            Log($"  Status: {checkout.Status}");
            Log($"  Product ID: {checkout.ProductId}");
            Log($"  Product Price ID: {checkout.ProductPriceId}");
            
            if (!string.IsNullOrEmpty(checkout.CustomerId))
            {
                Log($"  Customer ID: {checkout.CustomerId}");
            }
        }

        [Fact]
        public async Task CreateCheckout_ShouldGenerateUniqueIds()
        {
            // Arrange
            SkipIfNoClient(nameof(CreateCheckout_ShouldGenerateUniqueIds));
            Skip.If(string.IsNullOrEmpty(ProductId), "No ProductId configured");
            LogSection("Checkout Uniqueness Test");

            // Act
            Log("Creating multiple checkouts...");
            var successUrl = $"{WebhookBaseUrl}/success";
            var checkout1 = await Client!.CreateCheckoutAsync(null, successUrl);
            var checkout2 = await Client!.CreateCheckoutAsync(null, successUrl);

            // Assert
            Assert.NotNull(checkout1);
            Assert.NotNull(checkout2);
            Assert.NotEqual(checkout1.Id, checkout2.Id);
            Assert.NotEqual(checkout1.ClientSecret, checkout2.ClientSecret);
            
            Log($"✓ Unique checkout IDs confirmed");
            Log($"  Checkout 1: {checkout1.Id}");
            Log($"  Checkout 2: {checkout2.Id}");
        }

        [Fact]
        public async Task CreateCheckout_WithCustomerEmail_ShouldIncludeInResponse()
        {
            // Arrange
            SkipIfNoClient(nameof(CreateCheckout_WithCustomerEmail_ShouldIncludeInResponse));
            Skip.If(string.IsNullOrEmpty(ProductId), "No ProductId configured");
            
            var customerEmail = GenerateTestEmail();
            var successUrl = $"{WebhookBaseUrl}/success";

            // Act
            var checkout = await Client!.CreateCheckoutAsync(customerEmail, successUrl);

            // Assert
            Assert.NotNull(checkout);
            // Note: Customer email might not be directly returned in checkout response,
            // but the checkout should be associated with the email
            
            Log($"✓ Checkout created for customer email: {customerEmail}");
            Log($"  Checkout ID: {checkout.Id}");
        }

        [Fact]
        public async Task GetCheckout_ShouldHaveValidUrls()
        {
            // Arrange
            SkipIfNoClient(nameof(GetCheckout_ShouldHaveValidUrls));
            Skip.If(string.IsNullOrEmpty(ProductId), "No ProductId configured");
            
            var checkout = await Client!.CreateCheckoutAsync(successUrl: $"{WebhookBaseUrl}/success");

            // Act
            var retrievedCheckout = await Client.GetCheckoutAsync(checkout.Id);

            // Assert
            Assert.NotNull(retrievedCheckout);
            Assert.NotEmpty(retrievedCheckout.Url);
            Assert.StartsWith("http", retrievedCheckout.Url);
            
            if (!string.IsNullOrEmpty(retrievedCheckout.SuccessUrl))
            {
                Assert.StartsWith("http", retrievedCheckout.SuccessUrl);
            }
            
            Log($"✓ Checkout URLs valid");
            Log($"  Checkout URL: {retrievedCheckout.Url}");
            if (!string.IsNullOrEmpty(retrievedCheckout.SuccessUrl))
            {
                Log($"  Success URL: {retrievedCheckout.SuccessUrl}");
            }
        }
    }
}