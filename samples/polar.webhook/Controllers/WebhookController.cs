using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using PolarNet.Models;
using PolarNet.Services;
using Polar.Services;

namespace Polar.Controllers;

[ApiController]
[Route("api/webhook")]
public class WebhookController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<WebhookController> _logger;
    private readonly PolarWebhookService _webhookService;
    private readonly IPolarWebhookEventHandler _handler;
    private readonly IWebhookEventStore _store;

    public WebhookController(IConfiguration configuration, ILogger<WebhookController> logger, PolarWebhookService webhookService, IPolarWebhookEventHandler handler, IWebhookEventStore store)
    {
        _configuration = configuration;
        _logger = logger;
        _webhookService = webhookService;
        _handler = handler;
        _store = store;
    }

    // GET: api/webhook/test
    [HttpGet("test")]
    public IActionResult Test()
    {
        return Ok(new
        {
            status = "running",
            message = "Webhook server is running!",
            timestamp = DateTime.UtcNow,
            endpoints = new[]
            {
                "/api/webhook/test - test endpoint",
                "/api/webhook/polar - Polar webhook receive"
            }
        });
    }

    [HttpPost("polar")]
    public async Task<IActionResult> HandlePolarWebhook()
    {
        try
        {
            // Read the raw body
            Request.EnableBuffering();
            using var reader = new StreamReader(Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            Request.Body.Position = 0;

            // Get webhook headers
            var webhookId = Request.Headers["Polar-Webhook-Id"].FirstOrDefault() ?? Request.Headers["webhook-id"].FirstOrDefault();
            var webhookSignature = Request.Headers["Polar-Webhook-Signature"].FirstOrDefault() ?? Request.Headers["webhook-signature"].FirstOrDefault();
            var webhookTimestamp = Request.Headers["Polar-Webhook-Timestamp"].FirstOrDefault() ?? Request.Headers["webhook-timestamp"].FirstOrDefault();

            _logger.LogInformation("Received webhook - ID: {WebhookId}, Timestamp: {Timestamp}", 
                webhookId, webhookTimestamp);

            // Verify webhook signature (skips if secret not set)
            var isValid = _webhookService.VerifySignature(Encoding.UTF8.GetBytes(body), webhookSignature ?? string.Empty);
            if (!isValid)
            {
                _logger.LogWarning("Invalid webhook signature for event {WebhookId}", webhookId);
                return Unauthorized(new { error = "invalid_signature" });
            }

            // Parse the webhook payload using System.Text.Json
            var payload = _webhookService.Parse(body);
            if (payload == null)
            {
                _logger.LogError("❌ Webhook failed to parse payload");
                return BadRequest("Invalid payload");
            }
            _logger.LogInformation("Processing webhook event: {EventType}", payload.Type);
            await _webhookService.DispatchAsync(payload, _handler);

            // Store received event for E2E tests
            _store.Add(new WebhookEventRecord(
                payload.EventId,
                payload.Type,
                payload.CreatedAt == default ? DateTime.UtcNow : payload.CreatedAt,
                payload.Data
            ));

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

    // GET: api/webhook/events?sinceUtc=...&type=...&max=...
    [HttpGet("events")]
    public IActionResult ListEvents([FromQuery] DateTime? sinceUtc = null, [FromQuery] string? type = null, [FromQuery] int max = 200)
    {
        var since = sinceUtc ?? DateTime.UtcNow.AddMinutes(-30);
        var items = _store.List(since, type, max);
        return Ok(new { count = items.Count, items });
    }

    private async Task HandleCheckoutCreated(JsonElement data)
    {
        string? checkoutId = data.TryGetProperty("id", out var id) ? id.GetString() : null;
        string? status = data.TryGetProperty("status", out var s) ? s.GetString() : null;
        
        _logger.LogInformation("Checkout created - ID: {CheckoutId}, Status: {Status}", 
            checkoutId, status);
        
        // TODO: Implement your business logic here
        await Task.CompletedTask;
    }

    private async Task HandleCheckoutUpdated(JsonElement data)
    {
        string? checkoutId = data.TryGetProperty("id", out var id) ? id.GetString() : null;
        string? status = data.TryGetProperty("status", out var s) ? s.GetString() : null;
        
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
            
            if (string.IsNullOrEmpty(checkoutId))
            {
                _logger.LogWarning("Checkout ID is null or empty for a succeeded event. Skipping post-payment processing.");
            }
            else
            {
                await ProcessSuccessfulPayment(checkoutId, data);
            }
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
        string? orderId = data.TryGetProperty("id", out var id) ? id.GetString() : null;
        string? customerId = data.TryGetProperty("customer_id", out var cid) ? cid.GetString() : null;
        int amount = data.TryGetProperty("amount", out var a) ? a.GetInt32() : 0;
        string? currency = data.TryGetProperty("currency", out var c) ? c.GetString() : null;
        
        _logger.LogInformation("Order created - ID: {OrderId}, Customer: {CustomerId}, Amount: {Amount} {Currency}", 
            orderId, customerId, amount, currency);
        
        // TODO: Implement order processing logic
        await Task.CompletedTask;
    }

    private async Task HandleSubscriptionCreated(JsonElement data)
    {
        string? subscriptionId = data.TryGetProperty("id", out var id) ? id.GetString() : null;
        string? customerId = data.TryGetProperty("customer_id", out var cid) ? cid.GetString() : null;
        string? productId = data.TryGetProperty("product_id", out var pid) ? pid.GetString() : null;
        
        _logger.LogInformation("Subscription created - ID: {SubscriptionId}, Customer: {CustomerId}, Product: {ProductId}", 
            subscriptionId, customerId, productId);
        
        // TODO: Implement subscription activation logic
        await Task.CompletedTask;
    }

    private async Task HandleSubscriptionUpdated(JsonElement data)
    {
        string? subscriptionId = data.TryGetProperty("id", out var id) ? id.GetString() : null;
        string? status = data.TryGetProperty("status", out var s) ? s.GetString() : null;
        
        _logger.LogInformation("Subscription updated - ID: {SubscriptionId}, Status: {Status}", 
            subscriptionId, status);
        
        // TODO: Implement subscription update logic
        await Task.CompletedTask;
    }

    private async Task HandleSubscriptionCanceled(JsonElement data)
    {
        string? subscriptionId = data.TryGetProperty("id", out var id) ? id.GetString() : null;
        
        _logger.LogInformation("Subscription canceled - ID: {SubscriptionId}", subscriptionId);
        
        // TODO: Implement subscription cancellation logic
        await Task.CompletedTask;
    }

    private async Task HandleCustomerCreated(JsonElement data)
    {
        string? customerId = data.TryGetProperty("id", out var id) ? id.GetString() : null;
        string? email = data.TryGetProperty("email", out var e) ? e.GetString() : null;
        
        _logger.LogInformation("Customer created - ID: {CustomerId}, Email: {Email}", 
            customerId, email);
        
        // TODO: Implement customer creation logic
        await Task.CompletedTask;
    }

    private async Task HandleCustomerUpdated(JsonElement data)
    {
        string? customerId = data.TryGetProperty("id", out var id) ? id.GetString() : null;
        
        _logger.LogInformation("Customer updated - ID: {CustomerId}", customerId);
        
        // TODO: Implement customer update logic
        await Task.CompletedTask;
    }

    private async Task ProcessSuccessfulPayment(string checkoutId, dynamic checkoutData)
    {
        try
        {
            // Extract relevant information from the checkout data
            string? customerId = checkoutData.customer_id;
            string? customerEmail = checkoutData.customer_email;
            string? productId = checkoutData.product_id;

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

// Handler is now registered via DI in Program.cs (SampleWebhookHandler)