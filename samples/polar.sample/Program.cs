using Microsoft.Extensions.Configuration;
using PolarNet.Services;

// Load configuration from appsettings.json
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var polar = config.GetSection("PolarSettings");
var accessToken = polar["AccessToken"] ?? string.Empty;
var organizationId = polar["OrganizationId"];
var productId = polar["ProductId"];
var priceId = polar["PriceId"];
var useSandbox = string.Equals(polar["UseSandbox"], "true", StringComparison.OrdinalIgnoreCase);
var baseUrl = useSandbox ? polar["SandboxApiUrl"] : polar["ProductionApiUrl"];

if (string.IsNullOrWhiteSpace(accessToken))
{
    Console.WriteLine("PolarSettings:AccessToken is missing in appsettings.json");
    return;
}

var client = new PolarClient(new PolarClientOptions
{
    AccessToken = accessToken,
    BaseUrl = baseUrl ?? "",
    OrganizationId = organizationId,
    DefaultProductId = productId,
    DefaultPriceId = priceId,
});

try
{
    Console.WriteLine("=== Polar Client Demo ===");
    Console.WriteLine($"Base URL: {baseUrl}");

    // List products
    var products = await client.ListProductsAsync();
    Console.WriteLine($"Products: {products.Items.Count}");

    // Get product by id if available
    if (products.Items.Count > 0)
    {
        var pid = products.Items[0].Id;
        var product = await client.GetProductAsync(pid);
        Console.WriteLine($"First Product: {product.Name} ({product.Id})");
    }

    // List prices (optionally filtered by product)
    var prices = await client.ListPricesAsync(productId);
    Console.WriteLine($"Prices: {prices.Items.Count}");
    if (prices.Items.Count > 0)
    {
        var firstPriceId = prices.Items[0].Id;
        var price = await client.GetPriceAsync(firstPriceId);
        Console.WriteLine($"First Price: {price.Type} {price.PriceAmount} {price.PriceCurrency} (Id={price.Id})");
    }

    // List orders
    var orders = await client.ListOrdersAsync();
    Console.WriteLine($"Orders: {orders.Items.Count}");
    if (orders.Items.Count > 0)
    {
        var firstOrderId = orders.Items[0].Id;
        var order = await client.GetOrderAsync(firstOrderId);
        Console.WriteLine($"First Order: {order.Id}, Amount: {order.Amount} {order.Currency}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
