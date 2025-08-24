using System;
using System.Linq;
using System.Threading.Tasks;
using PolarNet.Tests.Base;
using PolarNet.Models;
using Xunit;
using Xunit.Abstractions;

namespace PolarNet.Tests.Categories
{
    /// <summary>
    /// Tests for Product and Price API endpoints.
    /// </summary>
    public class ProductTests : TestBase
    {
        public ProductTests(PolarSandboxFixture fixture, ITestOutputHelper output) 
            : base(fixture, output)
        {
        }

        [Fact]
        public async Task ListProducts_ShouldReturnProducts()
        {
            // Arrange
            SkipIfNoClient(nameof(ListProducts_ShouldReturnProducts));
            LogSection("List Products Test");

            // Act
            Log("Fetching products...");
            var products = await Client!.ListProductsAsync(1, 10);

            // Assert
            Assert.NotNull(products);
            Assert.NotNull(products.Items);
            
            if (products.Items.Count > 0)
            {
                Log($"✓ Found {products.Items.Count} products");
                
                foreach (var product in products.Items.Take(3))
                {
                    Assert.NotEmpty(product.Id);
                    Assert.NotEmpty(product.Name);
                    
                    Log($"  - {product.Name} (ID: {product.Id})");
                    Log($"    Recurring: {product.IsRecurring}, Archived: {product.IsArchived}");
                }
            }
            else
            {
                Log("⚠ No products found (this may be expected for a new organization)");
            }
        }

        [Fact]
        public async Task GetProduct_WithValidId_ShouldReturnProduct()
        {
            // Arrange
            SkipIfNoClient(nameof(GetProduct_WithValidId_ShouldReturnProduct));
            Skip.If(string.IsNullOrEmpty(ProductId), "No ProductId configured");
            LogSection("Get Product Test");

            // Act
            Log($"Fetching product {ProductId}...");
            var product = await Client!.GetProductAsync(ProductId);

            // Assert
            Assert.NotNull(product);
            Assert.Equal(ProductId, product.Id);
            Assert.NotEmpty(product.Name);
            
            Log($"✓ Product retrieved: {product.Name}");
            Log($"  Description: {product.Description ?? "No description"}");
            Log($"  Recurring: {product.IsRecurring}");
            Log($"  Recurring Interval: {product.RecurringInterval ?? "N/A"}");
            Log($"  Archived: {product.IsArchived}");
            Log($"  Created: {product.CreatedAt:yyyy-MM-dd}");
        }

        [Fact]
        public async Task GetProduct_WithDefaultId_ShouldReturnDefaultProduct()
        {
            // Arrange
            SkipIfNoClient(nameof(GetProduct_WithDefaultId_ShouldReturnDefaultProduct));
            Skip.If(string.IsNullOrEmpty(ProductId), "No default ProductId configured");
            
            // Act
            var product = await Client!.GetProductAsync(); // Uses default from config

            // Assert
            Assert.NotNull(product);
            Assert.Equal(ProductId, product.Id);
            
            Log($"✓ Default product retrieved: {product.Name}");
        }

        [Fact]
        public async Task ListPrices_ForProduct_ShouldReturnPrices()
        {
            // Arrange
            SkipIfNoClient(nameof(ListPrices_ForProduct_ShouldReturnPrices));
            Skip.If(string.IsNullOrEmpty(ProductId), "No ProductId configured");
            LogSection("List Prices Test");

            // Act
            Log($"Fetching prices for product {ProductId}...");
            var prices = await Client!.ListPricesAsync(ProductId!, 1, 10);

            // Assert
            Assert.NotNull(prices);
            Assert.NotNull(prices.Items);
            
            if (prices.Items.Count > 0)
            {
                Log($"✓ Found {prices.Items.Count} prices");
                
                foreach (var price in prices.Items)
                {
                    Assert.NotEmpty(price.Id);
                    Assert.NotEmpty(price.ProductId);
                    Assert.Equal(ProductId, price.ProductId);
                    
                    Log($"  - Price ID: {price.Id}");
                    Log($"    Amount: {price.PriceAmount} {price.PriceCurrency}");
                    Log($"    Type: {price.Type}");
                    
                    if (price.Type == "recurring" && !string.IsNullOrEmpty(price.RecurringInterval))
                    {
                        Log($"    Interval: {price.RecurringInterval}");
                    }
                }
            }
            else
            {
                Log("⚠ No prices found for product");
            }
        }

        [Fact]
        public async Task GetPrice_WithValidId_ShouldReturnPrice()
        {
            // Arrange
            SkipIfNoClient(nameof(GetPrice_WithValidId_ShouldReturnPrice));
            Skip.If(string.IsNullOrEmpty(PriceId), "No PriceId configured");
            LogSection("Get Price Test");

            // Act
            Log($"Fetching price {PriceId}...");
            var price = await Client!.GetPriceAsync(PriceId!);

            // Assert
            Assert.NotNull(price);
            Assert.Equal(PriceId, price.Id);
            Assert.NotEmpty(price.ProductId);
            Assert.True(price.PriceAmount > 0);
            Assert.NotEmpty(price.PriceCurrency);
            
            Log($"✓ Price retrieved: {price.Id}");
            Log($"  Product ID: {price.ProductId}");
            Log($"  Amount: {price.PriceAmount} {price.PriceCurrency}");
            Log($"  Type: {price.Type}");
            // Note: CreatedAt field is not available in PolarPrice model
        }

        [Fact]
        public async Task ListProducts_WithPagination_ShouldRespectLimits()
        {
            // Arrange
            SkipIfNoClient(nameof(ListProducts_WithPagination_ShouldRespectLimits));
            const int requestedLimit = 5;
            
            // Act
            var products = await Client!.ListProductsAsync(1, requestedLimit);

            // Assert
            Assert.NotNull(products);
            Assert.NotNull(products.Items);
            Assert.True(products.Items.Count <= requestedLimit, 
                $"Expected at most {requestedLimit} items, but got {products.Items.Count}");
            
            Log($"✓ Pagination respected: requested {requestedLimit}, received {products.Items.Count}");
        }

        [Fact]
        public async Task Product_Crud_Lifecycle_ShouldSucceed()
        {
            // Arrange
            SkipIfNoClient(nameof(Product_Crud_Lifecycle_ShouldSucceed));

            var create = new CreateProductRequest
            {
                Name = $"SDK Test Product {GenerateTestId()}",
                Description = "Created by automated test",
                IsRecurring = false
            };

            // Act - Create
            var product = await Client!.CreateProductAsync(create);
            Assert.NotNull(product);
            Assert.NotEmpty(product.Id);

            try
            {
                // Update
                var update = new UpdateProductRequest { Description = "Updated by test" };
                var updated = await Client.UpdateProductAsync(product.Id, update);
                Assert.NotNull(updated);
                Assert.Equal(product.Id, updated.Id);

                // Delete
                var deleted = await Client.DeleteProductAsync(product.Id);
                Assert.True(deleted);
            }
            finally
            {
                // Best-effort cleanup if delete failed
                try { await Client.DeleteProductAsync(product.Id); } catch { }
            }
        }
    }
}