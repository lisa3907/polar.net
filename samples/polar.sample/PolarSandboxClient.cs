using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.Json.Nodes;

public class PolarSandboxClient
{
    private readonly HttpClient _httpClient;
    private const string SANDBOX_BASE_URL = "https://sandbox-api.polar.sh";
    private readonly string _accessToken;
    private readonly string _organizationId = "<Org-id>";

    public PolarSandboxClient(string accessToken)
    {
        _accessToken = accessToken;
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(SANDBOX_BASE_URL);
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    // Get organization info
    public async Task<string> GetOrganization()
    {
        var response = await _httpClient.GetAsync($"/v1/organizations/{_organizationId}");
        return await response.Content.ReadAsStringAsync();
    }

    // List all products
    public async Task<string> ListProducts()
    {
        var response = await _httpClient.GetAsync($"/v1/products?organization_id={_organizationId}");
        var content = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Products found:");
            var json = JsonNode.Parse(content)?.AsObject();
            var items = json?["items"] as JsonArray;

            if (items != null && items.Count > 0)
            {
                foreach (var item in items)
                {
                    var id = item?["id"]?.ToString();
                    var name = item?["name"]?.ToString();
                    Console.WriteLine($"- ID: {id}, Name: {name}");
                }
            }
            else
            {
                Console.WriteLine("No products found. You need to create one first.");
            }
        }
        else
        {
            Console.WriteLine($"Error: {response.StatusCode} - {content}");
        }

        return content;
    }

    // Create product
    public async Task<string> CreateProduct(string name, string description)
    {
        var productData = new
        {
            organization_id = _organizationId,
            name = name,
            description = description,
            prices = new[]
            {
                new
                {
                    type = "recurring",
                    recurring_interval = "month",
                    price_amount = 10000, // $100.00 in cents
                    price_currency = "usd"
                }
            }
        };

        var json = JsonSerializer.Serialize(productData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/v1/products", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var productObj = JsonNode.Parse(responseContent)?.AsObject();

            Console.WriteLine("Product created successfully!");
            Console.WriteLine($"Product ID: {productObj?["id"]}");
            Console.WriteLine($"Product Name: {productObj?["name"]}");
        }
        else
        {
            Console.WriteLine($"Error creating product: {response.StatusCode}");
            Console.WriteLine(responseContent);
        }

        return responseContent;
    }

    // Get product by id
    public async Task<string> GetProduct(string productId)
    {
        var response = await _httpClient.GetAsync($"/v1/products/{productId}");
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Error getting product: {response.StatusCode}");
            Console.WriteLine(content);
        }

        return content;
    }
}