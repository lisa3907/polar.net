using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class PolarSandboxClient
{
    private readonly HttpClient _httpClient;
    private const string SANDBOX_BASE_URL = "https://sandbox-api.polar.sh";
    private readonly string _accessToken;
    private readonly string _organizationId = "e4231692-a863-4d07-832d-e0a83cc85cbd"; // ODINSOFT

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

    // Organization 정보 조회
    public async Task<string> GetOrganization()
    {
        var response = await _httpClient.GetAsync($"/v1/organizations/{_organizationId}");
        return await response.Content.ReadAsStringAsync();
    }

    // 모든 Product 조회
    public async Task<string> ListProducts()
    {
        var response = await _httpClient.GetAsync($"/v1/products?organization_id={_organizationId}");
        var content = await response.Content.ReadAsStringAsync();
        
        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Products found:");
            var json = JObject.Parse(content);
            var items = json["items"] as JArray;
            
            if (items != null && items.Count > 0)
            {
                foreach (var item in items)
                {
                    Console.WriteLine($"- ID: {item["id"]}, Name: {item["name"]}");
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

    // Product 생성
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

        var json = JsonConvert.SerializeObject(productData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/v1/products", content);
        var responseContent = await response.Content.ReadAsStringAsync();
        
        if (response.IsSuccessStatusCode)
        {
            var productObj = JObject.Parse(responseContent);
            Console.WriteLine($"Product created successfully!");
            Console.WriteLine($"Product ID: {productObj["id"]}");
            Console.WriteLine($"Product Name: {productObj["name"]}");
        }
        else
        {
            Console.WriteLine($"Error creating product: {response.StatusCode}");
            Console.WriteLine(responseContent);
        }
        
        return responseContent;
    }

    // 특정 Product 조회
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
