# PolarNet

[![NuGet](https://img.shields.io/nuget/v/PolarNet.svg)](https://www.nuget.org/packages/PolarNet/)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0-blue.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE.md)
[![Downloads](https://img.shields.io/nuget/dt/PolarNet.svg)](https://www.nuget.org/packages/PolarNet/)
[![API Coverage](https://img.shields.io/badge/API%20Coverage-45%25-yellow.svg)](docs/TASK.md)

 Thin C# client library for [Polar.sh](https://polar.sh) API - Modern billing infrastructure for developers.

 🚀 **v1.1.2** - Now with Payments, Refunds, and Webhook management!

 Supported frameworks: `.NET Standard 2.0`, `.NET Standard 2.1`, `.NET 8`, `.NET 9`

## 📋 Features

- ✅ **Core Payment Processing** - Checkout sessions, orders, payments, and refunds
- ✅ **Subscription Management** - Full subscription lifecycle with customer state tracking
- ✅ **Webhook Infrastructure** - Complete webhook endpoint management with signature verification
- ✅ **Customer Management** - CRUD operations with state and meter usage tracking
- ⚠️ **Products & Benefits** - Read-only access (full CRUD coming in v1.2.0)
- 🔜 **Coming Soon** - Discounts, License Keys, Files, OAuth2/OIDC authentication

## Project structure

```
polar.net/
├── src/                      # Class library (NuGet package)
│   ├── Models/               # Typed API models
│   │   ├── Resources/        # API resources (Product, Order, etc.)
│   │   ├── Requests/         # Request DTOs
│   │   ├── Webhooks/         # Webhook event models
│   │   └── CustomerState/    # Customer state models
│   ├── Services/             
│   │   ├── PolarClient/      # API endpoint implementations
│   │   └── Webhooks/         # Webhook handling services
│   └── polar.net.csproj
├── samples/
│   ├── polar.sample/         # Console app demonstrating API calls
│   └── polar.webhook/        # ASP.NET webhook receiver with event store
├── tests/                    # xUnit tests with 90%+ coverage
├── docs/                     
│   ├── TASK.md              # Implementation status vs official API
│   ├── ROADMAP.md           # Development roadmap
│   └── releases/            # Release notes
└── README.md
```

## 🚀 Quick start

### Install from NuGet

```powershell
dotnet add package PolarNet --version 1.1.2
```

Or via Package Manager:
```powershell
Install-Package PolarNet -Version 1.1.2
```

### Build from source

```powershell
git clone https://github.com/lisa3907/polar.net.git
cd polar.net
dotnet restore
dotnet build -c Release
```

### Run samples

```powershell
# Console sample
cd samples/polar.sample
dotnet run

# Webhook receiver (ASP.NET)
cd samples/polar.webhook
dotnet run
```

Notes:
- The samples read configuration from `appsettings.json` (no code edits required). Set your values in:
	- `samples/polar.sample/appsettings.json`
	- `samples/polar.webhook/appsettings.json`
- Start the ASP.NET webhook sample from `samples/polar.webhook` with `dotnet run`.

Minimal config used by samples/tests:

```json
{
	"PolarSettings": {
        "AccessToken": "<YOUR_SANDBOX_OAT>",
        "WebhookSecret": "<YOUR_WEBHOOK_SECRET>",
        "OrganizationId": "<YOUR_ORGANIZATION_ID>",
        "ProductId": "<YOUR_PRODUCT_ID>",
        "PriceId": "<YOUR_PRICE_ID>",
        "WebhookBaseUrl": "<YOUR_WEBHOOK_BASE_URL>",
        "SandboxApiUrl": "https://sandbox-api.polar.sh",
        "ProductionApiUrl": "https://api.polar.sh",
        "UseSandbox": true
	}
}
```

## 💻 Using the library

### Basic setup

```csharp
using PolarNet;

var client = new PolarClient(new PolarClientOptions
{
    AccessToken = "<YOUR_ACCESS_TOKEN>",
    BaseUrl = "https://sandbox-api.polar.sh", // or production URL
    OrganizationId = "<YOUR_ORG_ID>"
});

// Get organization details
var org = await client.GetOrganizationAsync();
Console.WriteLine($"Organization: {org.Name}");
```

### Example: Create checkout and process payment

```csharp
// Create a checkout session
var checkout = await client.CreateCheckoutAsync(new CreateCheckoutRequest
{
    OrganizationId = client.Options.OrganizationId,
    ProductId = "product_123",
    PriceId = "price_456",
    SuccessUrl = "https://example.com/success",
    CustomerEmail = "customer@example.com"
});

// Later, check payment status
var payments = await client.ListPaymentsAsync(new ListPaymentsRequest
{
    OrderId = checkout.OrderId
});

if (payments.Items.Any(p => p.Status == "succeeded"))
{
    Console.WriteLine("Payment successful!");
}
```

### Example: Handle webhooks

```csharp
// In your ASP.NET controller
[HttpPost("webhook")]
public async Task<IActionResult> HandleWebhook(
    [FromBody] string payload,
    [FromHeader(Name = "X-Polar-Signature")] string signature)
{
    var service = new PolarWebhookService("your_webhook_secret");
    
    if (!service.VerifySignature(payload, signature))
        return Unauthorized();
    
    var envelope = service.ParseEnvelope(payload);
    
    switch (envelope.Event)
    {
        case "order.created":
            var order = JsonSerializer.Deserialize<PolarOrder>(envelope.Data);
            // Process order...
            break;
        case "subscription.updated":
            var subscription = JsonSerializer.Deserialize<PolarSubscription>(envelope.Data);
            // Handle subscription change...
            break;
    }
    
    return Ok();
}
```

## 📊 API Implementation Status

### ✅ Fully Implemented (8/22 endpoints)
- **Checkout** - Create and retrieve custom checkout sessions
- **Customers** - Full CRUD operations with state management
- **Subscriptions** - Complete lifecycle management (create, list, get, cancel, revoke)
- **Orders** - List and retrieve order details
- **Refunds** - Create (full/partial), list, and retrieve refunds
- **Payments** - List and retrieve payment details with filtering
- **Webhook Endpoints** - Full CRUD + testing capabilities
- **Customer State** - Track usage meters and granted benefits

### ⚠️ Partially Implemented (3/22 endpoints)
- **Products** - Read-only (list, get) - *Full CRUD in v1.2.0*
- **Benefits** - List only - *Full management in v1.2.0*
- **Organizations** - Get only - *Update operations in v1.3.0*

### 🔜 Coming Soon (11/22 endpoints)
- **v1.2.0** - Discounts, License Keys, Files, Product CRUD
- **v1.3.0** - OAuth2/OIDC, Customer Portal, Sessions
- **v1.4.0** - Metrics, Events, Meters, Custom Fields

For detailed implementation status, see [docs/TASK.md](docs/TASK.md).  
For development roadmap, see [docs/ROADMAP.md](docs/ROADMAP.md).

## 🧪 Testing

### Test cards for Sandbox

Stripe test cards for payment testing:

| Card Number | Scenario |
|-------------|----------|
| `4242 4242 4242 4242` | Success |
| `4000 0000 0000 0002` | Failure |
| `4000 0025 0000 3155` | 3D Secure authentication |
| `4000 0000 0000 9995` | Decline |

### Running tests

```powershell
cd tests/PolarNet.Tests
dotnet test --logger "console;verbosity=detailed"
```

## 🔧 Troubleshooting

| Error | Cause | Solution |
|-------|-------|----------|
| `401 Unauthorized` | Invalid/expired token | Verify token is valid for environment (sandbox/production) |
| `404 Not Found` | Resource doesn't exist | Check IDs exist in your environment |
| `422 Unprocessable Entity` | Invalid request | Verify required fields and data types |
| `429 Too Many Requests` | Rate limit exceeded | Implement retry with exponential backoff |

## ⚙️ Configuration

### Authentication
- **Organization Access Token (OAT)** - Server-side operations
- **Customer Access Token** - Customer-specific operations (coming in v1.3.0)
- **OAuth2/OIDC** - Third-party integrations (coming in v1.3.0)

### Environment URLs
- **Production**: `https://api.polar.sh`
- **Sandbox**: `https://sandbox-api.polar.sh`

### Rate Limits
- 300 requests/minute per organization
- 1 request/second for license key validation

## 📚 Documentation

- 📖 [Implementation Status](docs/TASK.md) - Current API coverage details
- 🗺️ [Development Roadmap](docs/ROADMAP.md) - Future release plans
- 📝 [Release Notes](docs/releases/) - Version history and changes
- 🌐 [Polar API Reference](https://docs.polar.sh/api-reference) - Official API documentation
- 🧪 [Polar Sandbox](https://sandbox.polar.sh) - Test environment


## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

### Development setup
```powershell
git clone https://github.com/lisa3907/polar.net.git
cd polar.net
dotnet restore
dotnet build
dotnet test
```

## 👥 Team

### Core Development Team
- **SEONGAHN LEE** - Lead Developer & Project Architect ([lisa@odinsoft.co.kr](mailto:lisa@odinsoft.co.kr))
- **YUJIN** - Senior Developer & Integration Specialist ([yoojin@odinsoft.co.kr](mailto:yoojin@odinsoft.co.kr))
- **SEJIN** - Software Developer & API Implementation ([saejin@odinsoft.co.kr](mailto:saejin@odinsoft.co.kr))

## 📞 Support

- 🐛 **Issues**: [GitHub Issues](https://github.com/lisa3907/polar.net/issues)
- 💬 **Discussions**: [GitHub Discussions](https://github.com/lisa3907/polar.net/discussions)
- 📧 **Email**: help@odinsoft.co.kr
- 📖 **Documentation**: [Wiki](https://github.com/lisa3907/polar.net/wiki)

## 📄 License

MIT License - see [LICENSE.md](LICENSE.md) for details.

---

<div align="center">

**Built with ❤️ by the ODINSOFT Team**

[⭐ Star us on GitHub](https://github.com/lisa3907/polar.net) | 
[📦 NuGet Package](https://www.nuget.org/packages/PolarNet/) | 
[🌐 Polar.sh](https://polar.sh)

</div>