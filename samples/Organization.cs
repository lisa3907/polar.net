using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class Organization
{
    static async Task Run(string[] args)
    {
        string accessToken = "polar_oat_Rxv1IUDyhgYWNGJ5oKZmMBVluOmAkJRHpSXxb0Ai9x3";
        var client = new PolarSandboxClient(accessToken);
        
        Console.WriteLine("=== Organization Info ===");
        var org = await client.GetOrganization();
        Console.WriteLine(org);
        
        Console.WriteLine("\n=== Listing Products ===");
        var products = await client.ListProducts();
        
        // Product가 없으면 생성
        Console.WriteLine("\n=== Creating New Product (if needed) ===");
        var newProduct = await client.CreateProduct(
            "ODINSOFT Test Product",
            "ODINSOFT 테스트 구독 상품"
        );
        
        // 생성된 Product ID로 조회 테스트
        if (!string.IsNullOrEmpty(newProduct))
        {
            var productObj = JObject.Parse(newProduct);
            var productId = productObj["id"]?.ToString();
            
            if (!string.IsNullOrEmpty(productId))
            {
                Console.WriteLine($"\n=== Getting Product {productId} ===");
                var product = await client.GetProduct(productId);
                Console.WriteLine(product);
            }
        }
    }
}