// Enable nullable reference types locally for this file
#nullable enable
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PolarNet.Services;

namespace PolarNet;

/// <summary>
/// Simple entrypoint client for Polar API with opinionated defaults.
/// </summary>
public sealed class PolarClient : IDisposable
{
    private readonly HttpClient _http;
    private readonly PolarSandboxAPI _api;

    /// <summary>
    /// Construct using explicit <see cref="PolarClientOptions"/>.
    /// </summary>
    public PolarClient(PolarClientOptions options)
    {
    if (options is null) throw new ArgumentNullException(nameof(options));
        if (string.IsNullOrWhiteSpace(options.AccessToken))
            throw new ArgumentException("AccessToken is required", nameof(options));

        _http = new HttpClient
        {
            BaseAddress = new Uri(options.BaseUrl?.TrimEnd('/') ?? "https://sandbox-api.polar.sh")
        };
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.AccessToken);
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Bridge to existing service which expects IConfiguration
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PolarSettings:AccessToken"] = options.AccessToken,
                ["PolarSettings:OrganizationId"] = options.OrganizationId,
                ["PolarSettings:ProductId"] = options.DefaultProductId,
                ["PolarSettings:PriceId"] = options.DefaultPriceId,
                ["PolarSettings:UseSandbox"] = (options.BaseUrl ?? string.Empty).Contains("sandbox") ? "true" : "false",
                ["PolarSettings:SandboxApiUrl"] = options.BaseUrl,
                ["PolarSettings:ProductionApiUrl"] = options.BaseUrl,
            }!)
            .Build();

        _api = new PolarSandboxAPI(config);
    }

    /// <summary>
    /// Organization endpoints facade.
    /// </summary>
    public Task<Models.PolarOrganization> GetOrganizationAsync() => _api.GetOrganizationAsync();

    /// <summary>
    /// Product endpoints facade.
    /// </summary>
    public Task<Models.PolarProduct> GetProductAsync(string? productId = null) => _api.GetProductAsync(productId);
    /// <summary>List products for the configured organization (if set).</summary>
    public Task<Models.PolarListResponse<Models.PolarProduct>> ListProductsAsync() => _api.ListProductsAsync();

    /// <summary>
    /// Customer endpoints facade.
    /// </summary>
    public Task<Models.PolarCustomer> CreateCustomerAsync(string email, string? name = null) => _api.CreateCustomerAsync(email, name);
    /// <summary>List customers.</summary>
    public Task<Models.PolarListResponse<Models.PolarCustomer>> ListCustomersAsync() => _api.ListCustomersAsync();
    /// <summary>Get a customer by id.</summary>
    public Task<Models.PolarCustomer> GetCustomerAsync(string customerId) => _api.GetCustomerAsync(customerId);

    /// <summary>
    /// Subscription endpoints facade.
    /// </summary>
    public Task<Models.PolarSubscription> CreateSubscriptionAsync(string customerId, string? priceId = null) => _api.CreateSubscriptionAsync(customerId, priceId);
    /// <summary>List subscriptions.</summary>
    public Task<Models.PolarListResponse<Models.PolarSubscription>> ListSubscriptionsAsync() => _api.ListSubscriptionsAsync();
    /// <summary>Get a subscription by id.</summary>
    public Task<Models.PolarSubscription> GetSubscriptionAsync(string subscriptionId) => _api.GetSubscriptionAsync(subscriptionId);
    /// <summary>Cancel a subscription by id.</summary>
    public Task<bool> CancelSubscriptionAsync(string subscriptionId) => _api.CancelSubscriptionAsync(subscriptionId);

    /// <summary>
    /// Checkout endpoints facade.
    /// </summary>
    public Task<Models.PolarCheckout> CreateCheckoutAsync(string? customerEmail = null, string? successUrl = null) => _api.CreateCheckoutAsync(customerEmail, successUrl);
    /// <summary>Get a checkout session by id.</summary>
    public Task<Models.PolarCheckout> GetCheckoutAsync(string checkoutId) => _api.GetCheckoutAsync(checkoutId);

    /// <summary>
    /// Orders and Benefits.
    /// </summary>
    public Task<Models.PolarListResponse<Models.PolarOrder>> ListOrdersAsync() => _api.ListOrdersAsync();
    /// <summary>List available benefits.</summary>
    public Task<Models.PolarListResponse<Models.PolarBenefit>> ListBenefitsAsync() => _api.ListBenefitsAsync();
    
    /// <inheritdoc />
    public void Dispose()
    {
        _http.Dispose();
        _api.Dispose();
    }
}
