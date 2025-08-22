using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

public class Organization
{
    static async Task Run(string[] args)
    {
        string accessToken = "polar_oat_your_sandbox_token_here";
        var client = new PolarSandboxClient(accessToken);

        Console.WriteLine("=== Organization Info ===");
        var org = await client.GetOrganization();
        Console.WriteLine(org);

        Console.WriteLine("\n=== Listing Products ===");
        var products = await client.ListProducts();

        // Create a product if none exists
        Console.WriteLine("\n=== Creating New Product (if needed) ===");
        var newProduct = await client.CreateProduct(
            "Test Product",
            "Test Subscription Product"
        );

        // Fetch by the created Product ID
        if (!string.IsNullOrEmpty(newProduct))
        {
            var productObj = JsonNode.Parse(newProduct)?.AsObject();
            var productId = productObj?["id"]?.ToString();

            if (!string.IsNullOrEmpty(productId))
            {
                Console.WriteLine($"\n=== Getting Product {productId} ===");
                var product = await client.GetProduct(productId);
                Console.WriteLine(product);
            }
        }
    }
}