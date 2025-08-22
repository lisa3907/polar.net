# PolarNet Webhook Integration Guide

## Quick Start - Webhook Debugging in Visual Studio 2022

This guide provides a comprehensive walkthrough for setting up, testing, and debugging Polar webhooks in a local development environment using Visual Studio 2022.

## Prerequisites

Before starting, ensure you have the following installed:
- Visual Studio 2022 (17.8 or later)
- .NET 8.0 SDK
- PowerShell or Windows Terminal
- ngrok (for webhook tunneling)
- Polar Sandbox account

## Project Setup

### Step 1: Use the included sample project

This repo already contains `samples/polar.webhook` with controllers and services.
Open the solution (`polar.net.sln`) or folder in Visual Studio.

### Step 2: Open in Visual Studio 2022

There are two ways to open the project:
1. Double-click the `PolarWebhook.csproj` file
2. In Visual Studio: **File → Open → Project/Solution**

### Step 3: Configure settings

Create or edit `samples/polar.webhook/appsettings.json`:

```json
{
    "Logging": {
        "LogLevel": {
            "Default": "Information",
            "Microsoft.AspNetCore": "Warning"
        }
    },
    "PolarSettings": {
        "WebhookSecret": "",
        "UseSandbox": true,
        "SandboxApiUrl": "https://sandbox-api.polar.sh",
        "ProductionApiUrl": "https://api.polar.sh",
        "AccessToken": "<SANDBOX_OAT>",
        "OrganizationId": "<ORG_ID>"
    },
    "AllowedHosts": "*"
}
```

## Implementation

### Webhook Controller

Key excerpt from `samples/polar.webhook/Controllers/WebhookController.cs`:

```csharp
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;

namespace Polar.Controllers
{
    [ApiController]
    [Route("api/webhook")]
    public class WebhookController : ControllerBase
    {
    private readonly ILogger<WebhookController> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _webhookSecret;

        public WebhookController(
            IConfiguration configuration,
            ILogger<WebhookController> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _webhookSecret = _configuration["PolarSettings:WebhookSecret"] ?? "";
        }

        [HttpPost("polar")]
    public async Task<IActionResult> HandlePolarWebhook()
        {
            try
            {
        // Read raw body and headers
        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        Request.Body.Position = 0;

        var signature = Request.Headers["Polar-Webhook-Signature"].FirstOrDefault();
        var eventType = Request.Headers["Polar-Event-Type"].FirstOrDefault();
        var eventId = Request.Headers["Polar-Event-Id"].FirstOrDefault();

        // TODO: Verify signature when secret is configured
        if (!string.IsNullOrEmpty(_webhookSecret))
        {
            _logger.LogWarning("Webhook signature verification is not yet implemented");
        }

        var payload = JsonSerializer.Deserialize<PolarWebhookPayload>(body);
        _logger.LogInformation("Received webhook: Type={EventType}, Id={EventId}", eventType ?? payload?.Type, eventId);

        // TODO: Dispatch based on payload?.Type
        return Ok(new { success = true, eventId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook {EventId}", eventId);
                return StatusCode(500, "Internal server error");
            }
        }
    // Note: signature verification is pending
    }
}
```

### Webhook Service

Create `Services/WebhookService.cs`:

```csharp
using System.Text.Json;

namespace PolarWebhook.Services
{
    public interface IWebhookService
    {
        Task ProcessWebhookAsync(string eventType, JsonElement payload, string eventId);
    }

    public class WebhookService : IWebhookService
    {
        private readonly ILogger<WebhookService> _logger;
        private readonly string _logDirectory = "webhook-logs";

        public WebhookService(ILogger<WebhookService> logger)
        {
            _logger = logger;
            Directory.CreateDirectory(_logDirectory);
        }

        public async Task ProcessWebhookAsync(string eventType, JsonElement payload, string eventId)
        {
            // Log to file for debugging
            await LogWebhookToFileAsync(eventType, payload, eventId);

            // Process based on event type
            switch (eventType)
            {
                case "customer.created":
                    await HandleCustomerCreated(payload);
                    break;
                case "customer.updated":
                    await HandleCustomerUpdated(payload);
                    break;
                case "subscription.created":
                    await HandleSubscriptionCreated(payload);
                    break;
                case "subscription.updated":
                    await HandleSubscriptionUpdated(payload);
                    break;
                case "subscription.canceled":
                    await HandleSubscriptionCanceled(payload);
                    break;
                case "order.created":
                    await HandleOrderCreated(payload);
                    break;
                case "checkout.created":
                    await HandleCheckoutCreated(payload);
                    break;
                case "checkout.updated":
                    await HandleCheckoutUpdated(payload);
                    break;
                default:
                    _logger.LogInformation("Unhandled event type: {EventType}", eventType);
                    break;
            }
        }

        private async Task LogWebhookToFileAsync(string eventType, JsonElement payload, string eventId)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var filename = $"{timestamp}_{eventType}_{eventId}.json";
            var filepath = Path.Combine(_logDirectory, filename);
            
            var logContent = JsonSerializer.Serialize(new
            {
                eventId,
                eventType,
                timestamp = DateTime.UtcNow,
                payload
            }, new JsonSerializerOptions { WriteIndented = true });

            await File.WriteAllTextAsync(filepath, logContent);
            _logger.LogDebug("Webhook logged to file: {FilePath}", filepath);
        }

        private async Task HandleCustomerCreated(JsonElement payload)
        {
            _logger.LogInformation("Processing customer.created event");
            // Implement your business logic here
            await Task.CompletedTask;
        }

        private async Task HandleCustomerUpdated(JsonElement payload)
        {
            _logger.LogInformation("Processing customer.updated event");
            // Implement your business logic here
            await Task.CompletedTask;
        }

        private async Task HandleSubscriptionCreated(JsonElement payload)
        {
            _logger.LogInformation("Processing subscription.created event");
            // Implement your business logic here
            await Task.CompletedTask;
        }

        private async Task HandleSubscriptionUpdated(JsonElement payload)
        {
            _logger.LogInformation("Processing subscription.updated event");
            // Implement your business logic here
            await Task.CompletedTask;
        }

        private async Task HandleSubscriptionCanceled(JsonElement payload)
        {
            _logger.LogInformation("Processing subscription.canceled event");
            // Implement your business logic here
            await Task.CompletedTask;
        }

        private async Task HandleOrderCreated(JsonElement payload)
        {
            _logger.LogInformation("Processing order.created event");
            // Implement your business logic here
            await Task.CompletedTask;
        }

        private async Task HandleCheckoutCreated(JsonElement payload)
        {
            _logger.LogInformation("Processing checkout.created event");
            // Implement your business logic here
            await Task.CompletedTask;
        }

        private async Task HandleCheckoutUpdated(JsonElement payload)
        {
            _logger.LogInformation("Processing checkout.updated event");
            // Implement your business logic here
            await Task.CompletedTask;
        }
    }
}
```

### Program.cs Configuration

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IWebhookService, WebhookService>();

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### Application Settings

Create `appsettings.json` (sample):

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
    "PolarSettings": {
        "WebhookSecret": "",
        "UseSandbox": true,
        "SandboxApiUrl": "https://sandbox-api.polar.sh",
        "ProductionApiUrl": "https://api.polar.sh",
        "AccessToken": "<SANDBOX_OAT>",
        "OrganizationId": "<ORG_ID>"
    },
  "AllowedHosts": "*"
}
```

## Debugging Setup

### Step 1: Set Breakpoints in Visual Studio

Set breakpoints (F9) at these key locations:
- `WebhookController.cs` → Line with `VerifySignature` call
- `WebhookController.cs` → Line with `ProcessWebhookAsync` call
- `WebhookService.cs` → Beginning of `ProcessWebhookAsync` method
- `WebhookService.cs` → Each event handler method you want to debug

### Step 2: Start Debug Session

Press **F5** or click **Debug → Start Debugging**

You should see:
```
🚀 Now listening on: https://localhost:7XXX
📌 Now listening on: http://localhost:5XXX
🎯 Application started. Press Ctrl+C to shut down.
```

### Step 3: Setup ngrok Tunnel

Open a new terminal and run:
```bash
# Install ngrok if not already installed
# Download from: https://ngrok.com/download

# Start ngrok tunnel
ngrok http 5000

# You'll see output like:
# Session Status                online
# Forwarding                    https://abc123.ngrok-free.app -> http://localhost:5000
```

Copy the HTTPS forwarding URL (e.g., `https://abc123.ngrok-free.app`)

### Step 4: Configure Polar Sandbox

1. Navigate to [Polar Sandbox](https://sandbox.polar.sh)
2. Log in to your account
3. Go to **Settings → Webhooks**
4. Click **Add Webhook**
5. Configure the webhook:
    - **Endpoint URL**: `https://abc123.ngrok-free.app/api/webhook/polar`
    - **Secret**: leave empty for now (signature verification pending)
   - **Events**: Select all events you want to test
6. Click **Create Webhook**

### Step 5: Trigger Test Events

In Polar Sandbox, perform actions to trigger webhook events:

1. **Customer Events**:
   - Create a new customer → Triggers `customer.created`
   - Update customer details → Triggers `customer.updated`

2. **Product Events**:
   - Create a new product → Triggers `product.created`
   - Update product pricing → Triggers `product.updated`

3. **Subscription Events**:
   - Create a subscription → Triggers `subscription.created`
   - Cancel a subscription → Triggers `subscription.canceled`

4. **Order Events**:
   - Complete a purchase → Triggers `order.created`

When events are triggered, Visual Studio will automatically break at your breakpoints!

## Debugging Tools

### Visual Studio Debug Windows

1. **Locals Window** (Debug → Windows → Locals):
   - View all local variables in current scope
   - Inspect webhook payload structure
   - Check signature validation results

2. **Watch Window** (Debug → Windows → Watch):
   - Add specific expressions to monitor
   - Examples:
     - `payload.GetProperty("data")`
     - `eventType`
     - `signature`

3. **Call Stack** (Debug → Windows → Call Stack):
   - See the execution path
   - Navigate through the call hierarchy

4. **Immediate Window** (Debug → Windows → Immediate):
   - Execute code during debugging
   - Example: `JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true })`

### ngrok Inspector

Access the ngrok web interface at `http://127.0.0.1:4040`:
- View all webhook requests and responses
- Inspect headers and payloads
- Replay requests for testing
- Check response times and status codes

### Swagger UI

Access Swagger at `http://localhost:5000/swagger`:
- Test your webhook endpoint manually
- View API documentation
- Send test payloads

## Log Files

Check the `webhook-logs` directory for detailed webhook logs:
```
webhook-logs/
├── 20250822_143022_subscription.created_evt_123abc.json
├── 20250822_143125_order.created_evt_124def.json
└── 20250822_143230_customer.created_evt_125ghi.json
```

Each file contains:
- Event ID and type
- Timestamp
- Complete payload
- Processing status

## Advanced Debugging Techniques

### Conditional Breakpoints

Right-click a breakpoint and select **Conditions**:
```csharp
// Only break for specific event types
eventType == "subscription.created"

// Only break for specific customer IDs
payload.GetProperty("data").GetProperty("customer_id").GetString() == "cust_123"
```

### Tracepoints

Right-click a breakpoint and select **Actions**:
- Log messages without stopping execution
- Example: `Webhook received: {eventType} at {DateTime.Now}`

### Data Tips

Hover over variables during debugging to see:
- Current values
- Object properties
- Collection contents

### Exception Settings

Debug → Windows → Exception Settings:
- Configure which exceptions cause breaks
- Useful for catching specific error conditions

## Troubleshooting

### Common Issues and Solutions

| Issue | Solution |
|-------|----------|
| ngrok URL changes on restart | Update the webhook URL in Polar settings each time |
| Breakpoints not hitting | Ensure Debug mode (not Release), check if webhook is reaching your endpoint |
| Signature validation fails | Verify the secret matches in both Polar and your app settings |
| No webhooks received | Check ngrok is running, verify URL in Polar settings, check firewall |
| Timeout errors | Increase timeout in webhook processing, consider async processing |
| Missing events | Ensure events are selected in Polar webhook configuration |

### Debug Output

Enable detailed logging in `appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Debug",
      "PolarWebhook": "Trace"
    }
  }
}
```

## Best Practices

### Security
- Never commit webhook secrets to source control
- Use environment variables or secure configuration
- Validate signatures on every request (enable once implemented)
- Implement rate limiting
- Log security events

### Reliability
- Implement idempotency using event IDs
- Handle duplicate events gracefully
- Use retry logic for downstream services
- Implement circuit breakers
- Monitor webhook processing times

### Performance
- Process webhooks asynchronously
- Use background jobs for heavy processing
- Implement queuing for high volume
- Cache frequently accessed data
- Monitor memory usage

### Testing
- Create unit tests for signature verification
- Test each event handler independently
- Use integration tests for end-to-end flows
- Implement webhook replay for debugging
- Test error scenarios

## Production Considerations

### Deployment
- Use proper SSL certificates
- Implement health checks
- Set up monitoring and alerting
- Configure proper logging
- Use secrets management

### Scaling
- Use message queues (Azure Service Bus, RabbitMQ)
- Implement horizontal scaling
- Use distributed caching
- Consider serverless functions
- Implement load balancing

### Monitoring
- Track webhook processing times
- Monitor failure rates
- Set up alerts for anomalies
- Log all webhook events
- Implement dashboards

## Additional Resources

### Documentation
- [Polar Webhook Documentation](https://docs.polar.sh/webhooks)
- [ASP.NET Core Web API Documentation](https://docs.microsoft.com/aspnet/core/web-api)
- [ngrok Documentation](https://ngrok.com/docs)

### Tools
- [Webhook.site](https://webhook.site) - Test webhook endpoints
- [RequestBin](https://requestbin.com) - Inspect HTTP requests
- [Postman](https://www.postman.com) - API testing tool

### Community
- [Polar Discord](https://discord.gg/polar)
- [Stack Overflow](https://stackoverflow.com/questions/tagged/polar)
- [GitHub Discussions](https://github.com/polarsource/polar/discussions)

---

*Last Updated: 2025-08-23*
*Version: 1.0.0*