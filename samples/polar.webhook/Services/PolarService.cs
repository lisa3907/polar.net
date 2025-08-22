using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using PolarNet.Models;

namespace Polar.Services;

public interface IPolarService
{
    Task<PolarListResponse<PolarProduct>> GetProductsAsync(string? organizationId = null, bool isArchived = false);
    Task<PolarProduct?> GetProductByIdAsync(string productId);
    Task<PolarCheckout> CreateCheckoutSessionAsync(CreateCheckoutRequest request);
    Task<PolarCheckout?> GetCheckoutSessionAsync(string checkoutId);
    Task<PolarListResponse<PolarCustomer>> GetCustomersAsync(string? organizationId = null, int page = 1, int limit = 10);
    Task<PolarCustomer?> GetCustomerByIdAsync(string customerId);
    Task<PolarListResponse<PolarSubscription>> GetSubscriptionsAsync(string? organizationId = null, bool? active = null);
    Task<PolarSubscription?> GetSubscriptionByIdAsync(string subscriptionId);
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

        var polarConfig = _configuration.GetSection("PolarSettings");
        _accessToken = polarConfig["AccessToken"] ?? throw new InvalidOperationException("Polar AccessToken not configured");
        _organizationId = polarConfig["OrganizationId"];

        var useSandbox = polarConfig.GetValue<bool>("UseSandbox", true);
        var sandboxUrl = polarConfig["SandboxApiUrl"];
        var productionUrl = polarConfig["ProductionApiUrl"];
        _baseUrl = (useSandbox ? sandboxUrl : productionUrl) ?? "";

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

    public async Task<PolarListResponse<PolarProduct>> GetProductsAsync(string? organizationId = null, bool isArchived = false)
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
        var result = JsonSerializer.Deserialize<PolarListResponse<PolarProduct>>(json, _jsonOptions);

        return result ?? new PolarListResponse<PolarProduct>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error fetching products from Polar - URL: {BaseUrl}", _baseUrl);
            throw;
        }
    }

    public async Task<PolarProduct?> GetProductByIdAsync(string productId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/v1/products/{productId}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<PolarProduct>(json, _jsonOptions);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error fetching product {ProductId} from Polar", productId);
            throw;
        }
    }

    public async Task<PolarCheckout> CreateCheckoutSessionAsync(CreateCheckoutRequest request)
    {
        try
        {
        var successUrl = $"{_configuration["BaseUrl"]}/polar/success?checkout_id={{CHECKOUT_ID}}";

        // Ensure success_url is set on the request payload
        request.SuccessUrl = string.IsNullOrWhiteSpace(request.SuccessUrl) ? successUrl : request.SuccessUrl;

        var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_baseUrl}/v1/checkouts/custom/", content);
            response.EnsureSuccessStatusCode();

        var resultJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PolarCheckout>(resultJson, _jsonOptions)
             ?? throw new InvalidOperationException("Failed to create checkout session");

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

    public async Task<PolarCheckout?> GetCheckoutSessionAsync(string checkoutId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/v1/checkouts/custom/{checkoutId}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<PolarCheckout>(json, _jsonOptions);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error fetching checkout session {CheckoutId}", checkoutId);
            throw;
        }
    }

    public async Task<PolarListResponse<PolarCustomer>> GetCustomersAsync(string? organizationId = null, int page = 1, int limit = 10)
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
        var result = JsonSerializer.Deserialize<PolarListResponse<PolarCustomer>>(json, _jsonOptions);

        return result ?? new PolarListResponse<PolarCustomer>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error fetching customers from Polar");
            throw;
        }
    }

    public async Task<PolarCustomer?> GetCustomerByIdAsync(string customerId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/v1/customers/{customerId}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<PolarCustomer>(json, _jsonOptions);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error fetching customer {CustomerId} from Polar", customerId);
            throw;
        }
    }

    public async Task<PolarListResponse<PolarSubscription>> GetSubscriptionsAsync(string? organizationId = null, bool? active = null)
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
        var result = JsonSerializer.Deserialize<PolarListResponse<PolarSubscription>>(json, _jsonOptions);

        return result ?? new PolarListResponse<PolarSubscription>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error fetching subscriptions from Polar");
            throw;
        }
    }

    public async Task<PolarSubscription?> GetSubscriptionByIdAsync(string subscriptionId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/v1/subscriptions/{subscriptionId}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<PolarSubscription>(json, _jsonOptions);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error fetching subscription {SubscriptionId} from Polar", subscriptionId);
            throw;
        }
    }
}