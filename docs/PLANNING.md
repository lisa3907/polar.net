# Polar.NET - C# API Client Library Planning Document

## Overview

The Polar.NET project aims to create a comprehensive C# client library for the [Polar API](https://docs.polar.sh/api-reference/introduction), providing developers with a robust, type-safe, and intuitive way to integrate Polar's monetization platform into their .NET applications.

## Project Goals

### Primary Objectives
- Create a production-ready NuGet package for Polar API integration
- Implement full API coverage with strong typing and IntelliSense support
- Provide comprehensive documentation and examples
- Ensure cross-platform compatibility (.NET 6.0+)
- Maintain high code quality with extensive unit testing

### Secondary Objectives
- Support both synchronous and asynchronous operations
- Implement retry policies and error handling
- Provide webhook signature verification
- Support dependency injection patterns
- Include performance optimizations for large-scale applications

## Architecture Design

### Core Library Structure (`src/polar.net`)

```
src/
├── Models/
│   ├── Common/          # Shared models and enums
│   ├── Requests/        # API request DTOs
│   ├── Resources/       # Resource models (Customer, Product, etc.)
│   └── Webhooks/        # Webhook event models
├── Services/
│   ├── PolarClient.cs        # Main API client
│   ├── PolarClientOptions.cs # Configuration options
│   └── Endpoints/            # Service-specific implementations
│       ├── CustomerService.cs
│       ├── ProductService.cs
│       ├── OrderService.cs
│       ├── SubscriptionService.cs
│       ├── CheckoutService.cs
│       ├── BenefitService.cs
│       └── WebhookService.cs
├── Authentication/
│   └── PolarAuthHandler.cs   # Bearer token authentication
├── Exceptions/
│   ├── PolarException.cs     # Base exception class
│   └── PolarApiException.cs  # API-specific exceptions
└── Extensions/
    └── ServiceCollectionExtensions.cs # DI extensions
```

### Key Features Implementation

#### 1. Authentication
- Bearer token authentication with secure storage
- Support for both sandbox and production environments
- Automatic token refresh (if applicable)
- API key validation

#### 2. HTTP Client Management
- HttpClient factory pattern for proper lifecycle management
- Configurable timeout and retry policies
- Request/response logging capabilities
- Custom headers support

#### 3. Error Handling
- Comprehensive exception hierarchy
- Detailed error messages from API responses
- Rate limiting detection and handling
- Network error recovery strategies

#### 4. Serialization
- System.Text.Json for optimal performance
- Custom converters for complex types
- Nullable reference types support
- JSON naming policies (camelCase/snake_case)

## Sample Applications (`samples/`)

### Console Application (`polar.sample`)
Demonstrates basic API operations:
- Customer management (CRUD operations)
- Product catalog management
- Order processing
- Subscription lifecycle
- Checkout flow implementation
- Benefit grants management

### ASP.NET Core Web Application (`polar.webhook`)
Showcases webhook integration:
- Webhook endpoint setup
- Signature verification
- Event processing pipeline
- Event logging and monitoring
- Error handling and retry logic
- Real-time notifications

## Testing Strategy (`tests/`)

### Unit Tests
- Service method coverage (>90%)
- Model serialization/deserialization
- Authentication flow testing
- Error handling scenarios
- Mock HTTP responses

### Integration Tests
- API endpoint verification
- End-to-end workflows
- Webhook signature validation
- Rate limiting behavior
- Concurrent request handling

### Test Infrastructure
- XUnit test framework
- Moq for mocking dependencies
- FluentAssertions for readable assertions
- Test fixtures for shared setup
- In-memory test server for webhook testing

## Development Phases

### Phase 1: Foundation (Week 1-2)
- [ ] Project structure setup
- [ ] Core models implementation
- [ ] Basic HTTP client configuration
- [ ] Authentication mechanism
- [ ] Error handling framework

### Phase 2: Core Services (Week 2-3)
- [ ] Customer service implementation
- [ ] Product service implementation
- [ ] Order service implementation
- [ ] Basic unit tests
- [ ] Initial documentation

### Phase 3: Advanced Features (Week 3-4)
- [ ] Subscription management
- [ ] Checkout flow
- [ ] Benefit grants
- [ ] Webhook signature verification
- [ ] Comprehensive error handling

### Phase 4: Sample Applications (Week 4-5)
- [ ] Console application development
- [ ] ASP.NET Core webhook application
- [ ] Documentation and examples
- [ ] Integration tests

### Phase 5: Polish & Release (Week 5-6)
- [ ] Performance optimization
- [ ] Security audit
- [ ] Documentation completion
- [ ] NuGet package preparation
- [ ] CI/CD pipeline setup

## Technical Requirements

### Target Frameworks
- .NET 6.0 (LTS)
- .NET 7.0
- .NET 8.0 (Latest LTS)

### Dependencies
- System.Text.Json (>= 8.0.0)
- Microsoft.Extensions.Http (>= 8.0.0)
- Microsoft.Extensions.DependencyInjection.Abstractions (>= 8.0.0)
- Microsoft.Extensions.Options (>= 8.0.0)

### Development Dependencies
- XUnit (>= 2.6.0)
- Moq (>= 4.20.0)
- FluentAssertions (>= 6.12.0)
- Microsoft.AspNetCore.Mvc.Testing (>= 8.0.0)

## API Coverage Checklist

### Organizations
- [x] List organizations
- [x] Get organization
- [x] Update organization

### Products
- [x] List products
- [x] Create product
- [x] Get product
- [x] Update product
- [x] Archive product

### Prices
- [x] List prices
- [x] Create price
- [x] Get price
- [x] Update price

### Benefits
- [x] List benefits
- [x] Create benefit
- [x] Get benefit
- [x] Update benefit
- [x] Delete benefit
- [x] Grant benefit

### Customers
- [x] List customers
- [x] Create customer
- [x] Get customer
- [x] Update customer
- [x] Delete customer

### Orders
- [x] List orders
- [x] Get order
- [x] Get order invoice

### Subscriptions
- [x] List subscriptions
- [x] Create subscription
- [x] Get subscription
- [x] Update subscription
- [x] Cancel subscription

### Checkouts
- [x] Create checkout session
- [x] Get checkout
- [x] Update checkout
- [x] Confirm checkout

### Webhooks
- [x] Event parsing
- [x] Signature verification
- [x] Event type mapping
- [x] Retry handling

## Configuration Example

```csharp
// appsettings.json
{
  "Polar": {
    "ApiKey": "YOUR_API_KEY",
    "Environment": "Sandbox", // or "Production"
    "BaseUrl": "https://sandbox.polar.sh/api/v1",
    "Timeout": 30,
    "MaxRetryAttempts": 3,
    "EnableLogging": true
  }
}

// Program.cs
builder.Services.AddPolar(options =>
{
    options.ApiKey = configuration["Polar:ApiKey"];
    options.Environment = PolarEnvironment.Sandbox;
    options.Timeout = TimeSpan.FromSeconds(30);
});
```

## Usage Examples

### Basic Usage
```csharp
var client = new PolarClient(apiKey);

// Create a customer
var customer = await client.Customers.CreateAsync(new CreateCustomerRequest
{
    Email = "customer@example.com",
    Name = "John Doe"
});

// Create a product
var product = await client.Products.CreateAsync(new CreateProductRequest
{
    Name = "Premium Plan",
    Description = "Access to all features"
});

// Create a checkout
var checkout = await client.Checkouts.CreateAsync(new CreateCheckoutRequest
{
    ProductId = product.Id,
    CustomerEmail = customer.Email,
    SuccessUrl = "https://example.com/success",
    CancelUrl = "https://example.com/cancel"
});
```

### Webhook Handling
```csharp
[HttpPost("webhook")]
public async Task<IActionResult> HandleWebhook(
    [FromBody] JsonElement payload,
    [FromHeader(Name = "Polar-Webhook-Signature")] string signature)
{
    var isValid = _polarClient.Webhooks.VerifySignature(payload, signature);
    if (!isValid)
        return Unauthorized();

    var webhookEvent = _polarClient.Webhooks.ParseEvent(payload);
    
    switch (webhookEvent.Type)
    {
        case "customer.created":
            await HandleCustomerCreated(webhookEvent.Data);
            break;
        case "subscription.created":
            await HandleSubscriptionCreated(webhookEvent.Data);
            break;
        // ... handle other events
    }

    return Ok();
}
```

## Success Metrics

### Code Quality
- Test coverage > 90%
- Zero critical security vulnerabilities
- Consistent code style (enforced by .editorconfig)
- Comprehensive XML documentation

### Performance
- API call latency < 100ms (excluding network)
- Memory allocation optimized
- Connection pooling implemented
- Efficient JSON serialization

### Developer Experience
- IntelliSense support for all public APIs
- Clear error messages
- Comprehensive examples
- Active maintenance and support

## Deployment Strategy

### NuGet Package
- Package ID: `Polar.NET`
- Semantic versioning (SemVer)
- Automated release pipeline
- Symbol package for debugging
- Release notes generation

### Documentation
- README with quick start guide
- API reference documentation
- Sample projects in repository
- Migration guides for updates
- Troubleshooting guide

### Support
- GitHub Issues for bug reports
- Discussions for questions
- Pull request guidelines
- Security vulnerability reporting

## Future Enhancements

### Version 2.0
- GraphQL support
- Batch operations
- Caching layer
- Advanced analytics
- Event sourcing support

### Version 3.0
- Real-time updates (WebSocket)
- Offline mode with sync
- Multi-tenant support
- Custom metrics collection
- Plugin architecture

## References

- [Polar API Documentation](https://docs.polar.sh/api-reference/introduction)
- [.NET Design Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/)
- [NuGet Best Practices](https://docs.microsoft.com/en-us/nuget/create-packages/package-authoring-best-practices)
- [ASP.NET Core Webhook Pattern](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests)

## Contact & Support

- **Repository**: https://github.com/lisa3907/polar.net
- **Issues**: https://github.com/lisa3907/polar.net/issues
- **Documentation**: https://github.com/lisa3907/polar.net/wiki
- **NuGet Package**: https://www.nuget.org/packages/Polar.NET

---

*Last Updated: 2025-08-22*
*Version: 1.0.0*