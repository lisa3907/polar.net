using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Polar.Models;

namespace Polar.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebhookController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<WebhookController> _logger;
    private readonly string _webhookSecret;

    public WebhookController(IConfiguration configuration, ILogger<WebhookController> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _webhookSecret = _configuration["Polar:WebhookSecret"] ?? "";
    }

    [HttpPost("polar")]
    public async Task<IActionResult> HandlePolarWebhook()
    {
        try
        {
            // Read the raw body
            Request.EnableBuffering();
            using var reader = new StreamReader(Request.Body, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            Request.Body.Position = 0;

            // Get webhook headers
            var webhookId = Request.Headers["webhook-id"].FirstOrDefault();
            var webhookSignature = Request.Headers["webhook-signature"].FirstOrDefault();
            var webhookTimestamp = Request.Headers["webhook-timestamp"].FirstOrDefault();

            _logger.LogInformation("Received webhook - ID: {WebhookId}, Timestamp: {Timestamp}", 
                webhookId, webhookTimestamp);

            // Verify webhook signature if secret is configured
            if (!string.IsNullOrEmpty(_webhookSecret))
            {
                // TODO: Implement webhook signature verification
                // For now, we'll just log that verification is skipped
                _logger.LogWarning("Webhook signature verification is not yet implemented");
                
                // In production, you should verify the webhook signature
                // using a library like StandardWebhooks or implement
                // the verification according to Polar's documentation
            }

            // Parse the webhook payload
            var payload = JsonSerializer.Deserialize<JsonElement>(body);
            var eventType = payload.GetProperty("type").GetString();
            var data = payload.GetProperty("data");

            _logger.LogInformation("Processing webhook event: {EventType}", eventType);

            // Process different webhook event types
            switch (eventType)
            {
                case "checkout.created":
                    await HandleCheckoutCreated(data);
                    break;

                case "checkout.updated":
                    await HandleCheckoutUpdated(data);
                    break;

                case "order.created":
                    await HandleOrderCreated(data);
                    break;

                case "subscription.created":
                    await HandleSubscriptionCreated(data);
                    break;

                case "subscription.updated":
                    await HandleSubscriptionUpdated(data);
                    break;

                case "subscription.canceled":
                    await HandleSubscriptionCanceled(data);
                    break;

                case "customer.created":
                    await HandleCustomerCreated(data);
                    break;

                case "customer.updated":
                    await HandleCustomerUpdated(data);
                    break;

                default:
                    _logger.LogInformation("Unhandled event type: {EventType}", eventType);
                    break;
            }

            return Ok(new { received = true });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON in webhook payload");
            return BadRequest(new { error = "Invalid JSON payload" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    private async Task HandleCheckoutCreated(JsonElement data)
    {
        var checkoutId = data.GetProperty("id").GetString();
        var status = data.GetProperty("status").GetString();
        
        _logger.LogInformation("Checkout created - ID: {CheckoutId}, Status: {Status}", 
            checkoutId, status);
        
        // TODO: Implement your business logic here
        await Task.CompletedTask;
    }

    private async Task HandleCheckoutUpdated(JsonElement data)
    {
        var checkoutId = data.GetProperty("id").GetString();
        var status = data.GetProperty("status").GetString();
        
        _logger.LogInformation("Checkout updated - ID: {CheckoutId}, Status: {Status}", 
            checkoutId, status);

        if (status == "succeeded")
        {
            // Payment completed successfully
            _logger.LogInformation("Payment successful for checkout {CheckoutId}", checkoutId);
            
            // TODO: Implement your post-payment logic here
            // - Update order status
            // - Send confirmation email
            // - Provision access to service
            // - Update inventory
            
            await ProcessSuccessfulPayment(checkoutId, data);
        }
        else if (status == "confirmed")
        {
            _logger.LogInformation("Payment confirmed, awaiting processing for checkout {CheckoutId}", checkoutId);
        }
        else if (status == "expired")
        {
            _logger.LogInformation("Checkout expired - ID: {CheckoutId}", checkoutId);
        }
    }

    private async Task HandleOrderCreated(JsonElement data)
    {
        var orderId = data.GetProperty("id").GetString();
        var customerId = data.GetProperty("customer_id").GetString();
        var amount = data.GetProperty("amount").GetInt32();
        var currency = data.GetProperty("currency").GetString();
        
        _logger.LogInformation("Order created - ID: {OrderId}, Customer: {CustomerId}, Amount: {Amount} {Currency}", 
            orderId, customerId, amount, currency);
        
        // TODO: Implement order processing logic
        await Task.CompletedTask;
    }

    private async Task HandleSubscriptionCreated(JsonElement data)
    {
        var subscriptionId = data.GetProperty("id").GetString();
        var customerId = data.GetProperty("customer_id").GetString();
        var productId = data.GetProperty("product_id").GetString();
        
        _logger.LogInformation("Subscription created - ID: {SubscriptionId}, Customer: {CustomerId}, Product: {ProductId}", 
            subscriptionId, customerId, productId);
        
        // TODO: Implement subscription activation logic
        await Task.CompletedTask;
    }

    private async Task HandleSubscriptionUpdated(JsonElement data)
    {
        var subscriptionId = data.GetProperty("id").GetString();
        var status = data.GetProperty("status").GetString();
        
        _logger.LogInformation("Subscription updated - ID: {SubscriptionId}, Status: {Status}", 
            subscriptionId, status);
        
        // TODO: Implement subscription update logic
        await Task.CompletedTask;
    }

    private async Task HandleSubscriptionCanceled(JsonElement data)
    {
        var subscriptionId = data.GetProperty("id").GetString();
        
        _logger.LogInformation("Subscription canceled - ID: {SubscriptionId}", subscriptionId);
        
        // TODO: Implement subscription cancellation logic
        await Task.CompletedTask;
    }

    private async Task HandleCustomerCreated(JsonElement data)
    {
        var customerId = data.GetProperty("id").GetString();
        var email = data.GetProperty("email").GetString();
        
        _logger.LogInformation("Customer created - ID: {CustomerId}, Email: {Email}", 
            customerId, email);
        
        // TODO: Implement customer creation logic
        await Task.CompletedTask;
    }

    private async Task HandleCustomerUpdated(JsonElement data)
    {
        var customerId = data.GetProperty("id").GetString();
        
        _logger.LogInformation("Customer updated - ID: {CustomerId}", customerId);
        
        // TODO: Implement customer update logic
        await Task.CompletedTask;
    }

    private async Task ProcessSuccessfulPayment(string? checkoutId, JsonElement checkoutData)
    {
        try
        {
            // Extract relevant information from the checkout data
            var customerId = checkoutData.TryGetProperty("customer_id", out var customerIdProp) 
                ? customerIdProp.GetString() : null;
            
            var customerEmail = checkoutData.TryGetProperty("customer_email", out var emailProp) 
                ? emailProp.GetString() : null;
            
            var productId = checkoutData.TryGetProperty("product_id", out var productIdProp) 
                ? productIdProp.GetString() : null;

            _logger.LogInformation("Processing successful payment - Checkout: {CheckoutId}, Customer: {CustomerId}, Email: {Email}, Product: {ProductId}",
                checkoutId, customerId, customerEmail, productId);

            // TODO: Implement your business logic here
            // Examples:
            // - Create or update customer record in your database
            // - Grant access to paid features
            // - Send confirmation email
            // - Update inventory
            // - Generate license key
            // - Create subscription record

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing successful payment for checkout {CheckoutId}", checkoutId);
            throw;
        }
    }
}