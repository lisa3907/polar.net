using Microsoft.Extensions.Configuration;

namespace PolarSample.Demos
{
    /// <summary>
    /// Demonstrates Product and Price API operations.
    /// </summary>
    public class ProductDemo : DemoBase
    {
        public ProductDemo(IConfigurationRoot config) : base(config) { }

        public override async Task RunAsync()
        {
            WriteHeader("Product & Price Demo");

            try
            {
                // List all products
                WriteSubHeader("Listing Products");
                var products = await Client.ListProductsAsync();
                Console.WriteLine($"Total Products: {products.Items.Count}");

                string? pidToUse = ProductId;
                if (string.IsNullOrWhiteSpace(pidToUse) && products.Items.Count > 0)
                {
                    pidToUse = products.Items[0].Id;
                }

                if (!string.IsNullOrWhiteSpace(pidToUse))
                {
                    // Get specific product details
                    WriteSubHeader("Product Details");
                    var product = await Client.GetProductAsync(pidToUse);
                    Console.WriteLine($"Product: {product.Name} ({product.Id})");
                    Console.WriteLine($"Description: {product.Description}");
                    Console.WriteLine($"Is Recurring: {product.IsRecurring}");

                    // Show prices embedded in product
                    var embeddedPrices = product.Prices ?? new();
                    Console.WriteLine($"Embedded Prices: {embeddedPrices.Count}");
                    
                    if (embeddedPrices.Count > 0)
                    {
                        WriteSubHeader("Price Information");
                        foreach (var price in embeddedPrices)
                        {
                            Console.WriteLine($"  - Price ID: {price.Id}");
                            Console.WriteLine($"    Type: {price.Type}");
                            Console.WriteLine($"    Amount: {price.PriceAmount} {price.PriceCurrency}");
                            if (!string.IsNullOrEmpty(price.RecurringInterval))
                            {
                                Console.WriteLine($"    Interval: {price.RecurringInterval}");
                            }
                        }
                    }

                    // List all prices for the product
                    WriteSubHeader("All Prices for Product");
                    var prices = await Client.ListPricesAsync(pidToUse);
                    Console.WriteLine($"Total Prices: {prices.Items.Count}");
                    
                    foreach (var price in prices.Items.Take(3)) // Show first 3 prices
                    {
                        Console.WriteLine($"  - {price.Id}: {price.PriceAmount} {price.PriceCurrency} ({price.Type})");
                    }
                }
                else
                {
                    Console.WriteLine("No products available for demonstration.");
                }

                // Demonstrate getting a specific price if we have one
                if (!string.IsNullOrWhiteSpace(PriceId))
                {
                    WriteSubHeader("Specific Price Details");
                    var price = await Client.GetPriceAsync(PriceId);
                    Console.WriteLine($"Price: {price.PriceAmount} {price.PriceCurrency}");
                    Console.WriteLine($"Type: {price.Type}");
                    Console.WriteLine($"Product ID: {price.ProductId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Product Demo: {ex.Message}");
            }
        }
    }
}