using Microsoft.AspNetCore.Mvc;
using Polar.Models;
using Polar.Services;

namespace Polar.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PolarController : ControllerBase
{
    private readonly IPolarService _polarService;
    private readonly ILogger<PolarController> _logger;

    public PolarController(IPolarService polarService, ILogger<PolarController> logger)
    {
        _polarService = polarService;
        _logger = logger;
    }

    [HttpGet("products")]
    public async Task<IActionResult> GetProducts([FromQuery] bool isArchived = false)
    {
        try
        {
            var products = await _polarService.GetProductsAsync(isArchived: isArchived);
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products");
            return StatusCode(500, new { error = "Failed to retrieve products" });
        }
    }

    [HttpGet("products/{productId}")]
    public async Task<IActionResult> GetProduct(string productId)
    {
        try
        {
            var product = await _polarService.GetProductByIdAsync(productId);
            if (product == null)
                return NotFound(new { error = "Product not found" });

            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product {ProductId}", productId);
            return StatusCode(500, new { error = "Failed to retrieve product" });
        }
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> CreateCheckout([FromBody] CheckoutRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.ProductPriceId))
            {
                return BadRequest(new { error = "ProductPriceId is required" });
            }

            var checkoutSession = await _polarService.CreateCheckoutSessionAsync(request);
            
            _logger.LogInformation("Created checkout session {CheckoutId} for price {PriceId}", 
                checkoutSession.Id, request.ProductPriceId);

            // Return the checkout URL for client-side redirect
            return Ok(new 
            { 
                checkoutId = checkoutSession.Id,
                checkoutUrl = checkoutSession.Url,
                status = checkoutSession.Status
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating checkout session");
            return StatusCode(500, new { error = "Failed to create checkout session" });
        }
    }

    [HttpGet("checkout/{checkoutId}")]
    public async Task<IActionResult> GetCheckoutStatus(string checkoutId)
    {
        try
        {
            var checkout = await _polarService.GetCheckoutSessionAsync(checkoutId);
            if (checkout == null)
                return NotFound(new { error = "Checkout session not found" });

            return Ok(new
            {
                checkoutId = checkout.Id,
                status = checkout.Status,
                customerEmail = checkout.CustomerEmail,
                customerName = checkout.CustomerName,
                createdAt = checkout.CreatedAt,
                expiresAt = checkout.ExpiresAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving checkout session {CheckoutId}", checkoutId);
            return StatusCode(500, new { error = "Failed to retrieve checkout session" });
        }
    }

    [HttpGet("customers")]
    public async Task<IActionResult> GetCustomers([FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        try
        {
            var customers = await _polarService.GetCustomersAsync(page: page, limit: limit);
            return Ok(customers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customers");
            return StatusCode(500, new { error = "Failed to retrieve customers" });
        }
    }

    [HttpGet("customers/{customerId}")]
    public async Task<IActionResult> GetCustomer(string customerId)
    {
        try
        {
            var customer = await _polarService.GetCustomerByIdAsync(customerId);
            if (customer == null)
                return NotFound(new { error = "Customer not found" });

            return Ok(customer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer {CustomerId}", customerId);
            return StatusCode(500, new { error = "Failed to retrieve customer" });
        }
    }

    [HttpGet("subscriptions")]
    public async Task<IActionResult> GetSubscriptions([FromQuery] bool? active = null)
    {
        try
        {
            var subscriptions = await _polarService.GetSubscriptionsAsync(active: active);
            return Ok(subscriptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving subscriptions");
            return StatusCode(500, new { error = "Failed to retrieve subscriptions" });
        }
    }

    [HttpGet("subscriptions/{subscriptionId}")]
    public async Task<IActionResult> GetSubscription(string subscriptionId)
    {
        try
        {
            var subscription = await _polarService.GetSubscriptionByIdAsync(subscriptionId);
            if (subscription == null)
                return NotFound(new { error = "Subscription not found" });

            return Ok(subscription);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving subscription {SubscriptionId}", subscriptionId);
            return StatusCode(500, new { error = "Failed to retrieve subscription" });
        }
    }
}