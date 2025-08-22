using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

// Read access token from environment to avoid hardcoding secrets
var accessToken = "polar_oat_Rxv1IUDyhgYWNGJ5oKZmMBVluOmAkJRHpSXxb0Ai9x3";
var userId = "e4231692-a863-4d07-832d-e0a83cc85cbd";
var baseUrl = "https://sandbox-api.polar.sh/v1"; // per docs: sandbox base URL includes /v1

using var httpClient = new HttpClient();

// Validate token presence
if (string.IsNullOrWhiteSpace(accessToken))
{
    Console.WriteLine("POLAR_ACCESS_TOKEN 환경 변수가 설정되지 않았습니다. Sandbox에서 발급한 OAT를 설정하세요.");
    Console.WriteLine("Docs: https://docs.polar.sh/integrate/authentication");
    return;
}

// Authorization/Accept 헤더 설정 (typed)
httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
httpClient.DefaultRequestHeaders.Accept.Clear();
httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

try
{
    Console.WriteLine("=== Testing Polar API Connection ===");
    Console.WriteLine($"Base URL: {baseUrl}");
    Console.WriteLine($"Token preview: {(accessToken.Length >= 12 ? accessToken.Substring(0, 12) : "<short>")}*** (len={accessToken.Length})");
    Console.WriteLine();

    // 1. 제품 목록 테스트
    Console.WriteLine("1. Testing /v1/products endpoint...");
    var productsUrl = $"{baseUrl}/products/{userId}";
    var response = await httpClient.GetAsync(productsUrl);
    
    Console.WriteLine($"   Status: {response.StatusCode}");
    var content = await response.Content.ReadAsStringAsync();
    
    if (!response.IsSuccessStatusCode)
    {
        Console.WriteLine($"   Error: {content}");
        if ((int)response.StatusCode == 401)
        {
            Console.WriteLine("   401 Unauthorized detected. Common causes:");
            Console.WriteLine("   - Token revoked (tokens leaked in code are auto-revoked by Polar)");
            Console.WriteLine("   - Using production token against sandbox or vice versa");
            Console.WriteLine("   - Missing scope: products:read");
            if (response.Headers.WwwAuthenticate is not null)
            {
                foreach (var h in response.Headers.WwwAuthenticate)
                {
                    Console.WriteLine($"   WWW-Authenticate: {h.Scheme} {h.Parameter}");
                }
            }
        }
        
        // 2. Organization 목록 테스트 (토큰 검증)
    Console.WriteLine("\n2. Testing /v1/organizations endpoint...");
        var orgsUrl = $"{baseUrl}/organizations";
        var orgResponse = await httpClient.GetAsync(orgsUrl);
        Console.WriteLine($"   Status: {orgResponse.StatusCode}");
        
        if (!orgResponse.IsSuccessStatusCode)
        {
            var orgContent = await orgResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"   Error: {orgContent}");
        }
        else
        {
            var orgContent = await orgResponse.Content.ReadAsStringAsync();
            var orgJson = JsonDocument.Parse(orgContent);
            Console.WriteLine($"   Success! Organizations found.");
            
            // Organization ID 추출
            if (orgJson.RootElement.TryGetProperty("items", out var items) && items.GetArrayLength() > 0)
            {
                var firstOrg = items[0];
                if (firstOrg.TryGetProperty("id", out var orgId))
                {
                    Console.WriteLine($"   Organization ID: {orgId.GetString()}");
                    
                    // 3. Organization ID를 사용하여 제품 조회
                    Console.WriteLine($"\n3. Testing products with organization_id...");
                    var productsWithOrgUrl = $"{baseUrl}/products?organization_id={orgId.GetString()}";
                    var prodOrgResponse = await httpClient.GetAsync(productsWithOrgUrl);
                    Console.WriteLine($"   Status: {prodOrgResponse.StatusCode}");
                    
                    if (prodOrgResponse.IsSuccessStatusCode)
                    {
                        var prodContent = await prodOrgResponse.Content.ReadAsStringAsync();
                        var prodJson = JsonDocument.Parse(prodContent);
                        if (prodJson.RootElement.TryGetProperty("items", out var prodItems))
                        {
                            Console.WriteLine($"   Products found: {prodItems.GetArrayLength()}");
                            if (prodItems.GetArrayLength() > 0)
                            {
                                var firstProd = prodItems[0];
                                if (firstProd.TryGetProperty("id", out var pid))
                                {
                                    var productId = pid.GetString();
                                    Console.WriteLine($"   Fetching product by id: {productId}");
                                    var productByIdUrl = $"{baseUrl}/products/{productId}";
                                    var productByIdResp = await httpClient.GetAsync(productByIdUrl);
                                    Console.WriteLine($"   /products/{'{'}id{'}'} Status: {productByIdResp.StatusCode}");
                                    if (!productByIdResp.IsSuccessStatusCode)
                                    {
                                        var byIdErr = await productByIdResp.Content.ReadAsStringAsync();
                                        Console.WriteLine($"   /products/{'{'}id{'}'} Error: {byIdErr}");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    else
    {
        Console.WriteLine("   Success!");
        var json = JsonDocument.Parse(content);
        if (json.RootElement.TryGetProperty("items", out var items))
        {
            Console.WriteLine($"   Products found: {items.GetArrayLength()}");
            // If we have at least one product, fetch by id to demonstrate /products/{id}
            if (items.GetArrayLength() > 0)
            {
                var first = items[0];
                if (first.TryGetProperty("id", out var pid))
                {
                    var productId = pid.GetString();
                    Console.WriteLine($"   Fetching product by id: {productId}");
                    var productByIdUrl = $"{baseUrl}/products/{productId}";
                    var productByIdResp = await httpClient.GetAsync(productByIdUrl);
                    Console.WriteLine($"   /products/{'{'}id{'}'} Status: {productByIdResp.StatusCode}");
                    if (!productByIdResp.IsSuccessStatusCode)
                    {
                        var byIdErr = await productByIdResp.Content.ReadAsStringAsync();
                        Console.WriteLine($"   /products/{'{'}id{'}'} Error: {byIdErr}");
                    }
                }
            }
        }
    }
    
    Console.WriteLine("\n=== Test Summary ===");
    Console.WriteLine("If you see 401 errors:");
    Console.WriteLine("1. Verify your token is from the SANDBOX environment");
    Console.WriteLine("2. Check token permissions (needs products:read at minimum)");
    Console.WriteLine("3. Ensure the token hasn't expired");
    Console.WriteLine("4. Visit: https://sandbox.polar.sh/settings/tokens");
}
catch (Exception ex)
{
    Console.WriteLine($"Exception: {ex.Message}");
    Console.WriteLine($"Stack: {ex.StackTrace}");
}
