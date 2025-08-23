using Microsoft.Extensions.Configuration;
using PolarNet.Services;

// Load configuration from appsettings.json

var configBuilder = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
#if DEBUG
configBuilder = configBuilder.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
#endif
var config = configBuilder.Build();

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


// Helper: Delete all customers
async Task DeleteAllCustomersAsync(PolarClient client)
{
    Console.WriteLine("Listing all customers...");
    int page = 1, limit = 100, totalDeleted = 0;
    while (true)
    {
        var customers = await client.ListCustomersAsync(page, limit);
        if (customers.Items.Count == 0) break;
        foreach (var c in customers.Items)
        {
            Console.Write($"Deleting customer {c.Id} ({c.Email})... ");
            bool ok = await client.DeleteCustomerAsync(c.Id);
            Console.WriteLine(ok ? "OK" : "FAILED");
            if (ok) totalDeleted++;
        }
        if (customers.Items.Count < limit) break;
        page++;
    }
    Console.WriteLine($"Total customers deleted: {totalDeleted}");
}

try
{
    Console.WriteLine("=== Polar Client Demo ===");
    Console.WriteLine($"Base URL: {baseUrl}");


    // Menu: 1. Product/order demo, 2. Delete ALL customers, 3. Create customer, 4. List customers, 5. Delete customer by ID
    Console.WriteLine("Select an action:");
    Console.WriteLine("1. Run product/order demo");
    Console.WriteLine("2. Delete ALL customers");
    Console.WriteLine("3. Create customer");
    Console.WriteLine("4. List customers");
    Console.WriteLine("5. Delete customer by ID");
    Console.Write("Enter choice [1-5]: ");
    var choice = Console.ReadLine();

    if (choice == "2")
    {
        await DeleteAllCustomersAsync(client);
        return;
    }
    if (choice == "3")
    {
        Console.Write("Enter customer email: ");
        var email = Console.ReadLine();
        Console.Write("Enter customer name (optional): ");
        var name = Console.ReadLine();
        var customer = await client.CreateCustomerAsync(email!, string.IsNullOrWhiteSpace(name) ? null : name);
        Console.WriteLine($"Created customer: {customer.Id} {customer.Email} {customer.Name}");
        return;
    }
    if (choice == "4")
    {
        var customers = await client.ListCustomersAsync(1, 100);
        Console.WriteLine($"Total customers: {customers.Items.Count}");
        foreach (var c in customers.Items)
        {
            Console.WriteLine($"- {c.Id}: {c.Email} {c.Name}");
        }
        return;
    }
    if (choice == "5")
    {
        Console.Write("Enter customer ID to delete: ");
        var id = Console.ReadLine();
        bool ok = await client.DeleteCustomerAsync(id!);
        Console.WriteLine(ok ? "Deleted successfully." : "Delete failed.");
        return;
    }

    // Product/order demo
    var products = await client.ListProductsAsync();
    Console.WriteLine($"Products: {products.Items.Count}");

    string? pidToUse = productId;
    if (string.IsNullOrWhiteSpace(pidToUse) && products.Items.Count > 0)
    {
        pidToUse = products.Items[0].Id;
    }

    if (!string.IsNullOrWhiteSpace(pidToUse))
    {
        var product = await client.GetProductAsync(pidToUse);
        Console.WriteLine($"Product: {product.Name} ({product.Id})");

        var embeddedPrices = product.Prices ?? new();
        Console.WriteLine($"Prices on product: {embeddedPrices.Count}");
        if (embeddedPrices.Count > 0)
        {
            var p = embeddedPrices[0];
            Console.WriteLine($"First Price: {p.Type} {p.PriceAmount} {p.PriceCurrency} (Id={p.Id}, Interval={p.RecurringInterval})");
        }
    }

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
