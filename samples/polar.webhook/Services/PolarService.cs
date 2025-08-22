using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Polar.Models;

namespace Polar.Services;

public interface IPolarService
{
    Task<PagedResponse<Product>> GetProductsAsync(string? organizationId = null, bool isArchived = false);
    Task<Product?> GetProductByIdAsync(string productId);
    Task<CheckoutResponse> CreateCheckoutSessionAsync(CheckoutRequest request);
    Task<CheckoutResponse?> GetCheckoutSessionAsync(string checkoutId);
    Task<PagedResponse<Customer>> GetCustomersAsync(string? organizationId = null, int page = 1, int limit = 10);
    Task<Customer?> GetCustomerByIdAsync(string customerId);
    Task<PagedResponse<Subscription>> GetSubscriptionsAsync(string? organizationId = null, bool? active = null);
    Task<Subscription?> GetSubscriptionByIdAsync(string subscriptionId);
}

public class PolarService : IPolarService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PolarService> _logger;
    private readonly string _baseUrl;
    private readonly string _accessToken;
    private readonly string? _organizationId;
    private readonly JsonSerializerOptions _jsonOptions;

    public PolarService(HttpClient httpClient, IConfiguration configuration, ILogger<PolarService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        var polarConfig = _configuration.GetSection("Polar");
        _accessToken = polarConfig["AccessToken"] ?? throw new InvalidOperationException("Polar AccessToken not configured");
        _organizationId = polarConfig["OrganizationId"];
        
        var useSandbox = polarConfig.GetValue<bool>("UseSandbox", true);
        _baseUrl = useSandbox ? "https://sandbox-api.polar.sh" : "https://api.polar.sh";

        // Clear any existing headers first
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        
        // Log configuration for debugging
        _logger.LogInformation("PolarService initialized - Sandbox: {UseSandbox}, BaseUrl: {BaseUrl}, OrgId: {OrgId}", 
            useSandbox, _baseUrl, _organizationId);
        _logger.LogDebug("Using Access Token: {TokenPreview}...", 
            _accessToken.Length > 20 ? _accessToken.Substring(0, 20) : _accessToken);

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        };
    }

    public async Task<PagedResponse<Product>> GetProductsAsync(string? organizationId = null, bool isArchived = false)
    {
        try
        {
            var orgId = organizationId ?? _organizationId;
            var url = $"{_baseUrl}/v1/products";
            
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(orgId))
            {
                queryParams.Add($"organization_id={orgId}");
            }
            queryParams.Add($"is_archived={isArchived.ToString().ToLower()}");

            if (queryParams.Any())
            {
                url += "?" + string.Join("&", queryParams);
            }

            _logger.LogInformation("Fetching products from: {Url}", url);
            
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Polar API error - Status: {StatusCode}, Content: {Content}", 
                    response.StatusCode, errorContent);
            }
            
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<PagedResponse<Product>>(json, _jsonOptions);
            
            return result ?? new PagedResponse<Product>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error fetching products from Polar - URL: {BaseUrl}", _baseUrl);
            throw;
        }
    }

    public async Task<Product?> GetProductByIdAsync(string productId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/v1/products/{productId}");
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Product>(json, _jsonOptions);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error fetching product {ProductId} from Polar", productId);
            throw;
        }
    }

    public async Task<CheckoutResponse> CreateCheckoutSessionAsync(CheckoutRequest request)
    {
        try
        {
            var successUrl = $"{_configuration["BaseUrl"]}/polar/success?checkout_id={{CHECKOUT_ID}}";
            
            var checkoutData = new
            {
                product_price_id = request.ProductPriceId,
                success_url = successUrl,
                payment_processor = "stripe",
                customer_email = request.CustomerEmail,
                customer_name = request.CustomerName,
                external_customer_id = request.ExternalCustomerId,
                allow_discount_codes = request.AllowDiscountCodes,
                require_billing_address = request.RequireBillingAddress,
                metadata = request.Metadata
            };

            var json = JsonSerializer.Serialize(checkoutData, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/v1/checkouts/custom/", content);
            response.EnsureSuccessStatusCode();

            var resultJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<CheckoutResponse>(resultJson, _jsonOptions);
            
            if (result == null)
                throw new InvalidOperationException("Failed to create checkout session");

            _logger.LogInformation("Created checkout session {CheckoutId} with status {Status}", 
                result.Id, result.Status);

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error creating checkout session");
            throw;
        }
    }

    public async Task<CheckoutResponse?> GetCheckoutSessionAsync(string checkoutId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/v1/checkouts/custom/{checkoutId}");
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<CheckoutResponse>(json, _jsonOptions);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error fetching checkout session {CheckoutId}", checkoutId);
            throw;
        }
    }

    public async Task<PagedResponse<Customer>> GetCustomersAsync(string? organizationId = null, int page = 1, int limit = 10)
    {
        try
        {
            var orgId = organizationId ?? _organizationId;
            var queryParams = new List<string>
            {
                $"page={page}",
                $"limit={limit}"
            };

            if (!string.IsNullOrEmpty(orgId))
            {
                queryParams.Add($"organization_id={orgId}");
            }

            var url = $"{_baseUrl}/v1/customers?" + string.Join("&", queryParams);

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<PagedResponse<Customer>>(json, _jsonOptions);
            
            return result ?? new PagedResponse<Customer>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error fetching customers from Polar");
            throw;
        }
    }

    public async Task<Customer?> GetCustomerByIdAsync(string customerId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/v1/customers/{customerId}");
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Customer>(json, _jsonOptions);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error fetching customer {CustomerId} from Polar", customerId);
            throw;
        }
    }

    public async Task<PagedResponse<Subscription>> GetSubscriptionsAsync(string? organizationId = null, bool? active = null)
    {
        try
        {
            var orgId = organizationId ?? _organizationId;
            var queryParams = new List<string>();

            if (!string.IsNullOrEmpty(orgId))
            {
                queryParams.Add($"organization_id={orgId}");
            }

            if (active.HasValue)
            {
                queryParams.Add($"active={active.Value.ToString().ToLower()}");
            }

            var url = $"{_baseUrl}/v1/subscriptions";
            if (queryParams.Any())
            {
                url += "?" + string.Join("&", queryParams);
            }

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<PagedResponse<Subscription>>(json, _jsonOptions);
            
            return result ?? new PagedResponse<Subscription>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error fetching subscriptions from Polar");
            throw;
        }
    }

    public async Task<Subscription?> GetSubscriptionByIdAsync(string subscriptionId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/v1/subscriptions/{subscriptionId}");
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Subscription>(json, _jsonOptions);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error fetching subscription {SubscriptionId} from Polar", subscriptionId);
            throw;
        }
    }
}