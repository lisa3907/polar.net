# PolarNet - C# API Client Library Planning Document

## Overview

The PolarNet project aims to create a comprehensive C# client library for the [Polar API](https://docs.polar.sh/api-reference/introduction), providing developers with a robust, type-safe, and intuitive way to integrate Polar's monetization platform into their .NET applications.

## Project Goals

### Primary Objectives
- Create a production-ready NuGet package for Polar API integration
- Implement full API coverage with strong typing and IntelliSense support
- Provide comprehensive documentation and examples
- Ensure cross-platform compatibility (netstandard2.0+, .NET 8/9)
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
    └── ServiceCollectionExtensions.cs # DI extensions (future)
```

### Key Features Implementation

#### 1. Authentication
- Bearer token authentication with secure storage
- Support for both sandbox and production environments
- Automatic token refresh (if applicable)
- API key validation

#### 2. HTTP Client Management
- Simple HttpClient usage within PolarClient; factory integration can be added later
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
- Signature verification (planned; stubbed in controller with TODO)
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
- Moq (optional) for mocking dependencies; tests currently use a custom HttpMessageHandler
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
- netstandard2.0
- netstandard2.1
- .NET 8.0
- .NET 9.0

### Dependencies (library)
- System.Net.Http (4.3.4)
- Microsoft.Extensions.Configuration (8.0.0)
- Microsoft.Extensions.Configuration.Json (8.0.0)
- System.Text.Json (inbox for .NET 8/9; available in runtime)

### Development Dependencies
- xUnit
- (Optional) Moq / FluentAssertions

## API Coverage Checklist (current)

### Organizations
- [ ] List organizations
- [x] Get organization
- [ ] Update organization

### Products
- [x] List products
- [ ] Create product
- [x] Get product
- [ ] Update product
- [ ] Archive product

### Prices
- [x] List prices
- [ ] Create price
- [x] Get price
- [ ] Update price

### Benefits
- [x] List benefits
- [ ] Create benefit
- [ ] Get benefit
- [ ] Update benefit
- [ ] Delete benefit
- [ ] Grant benefit

### Customers
- [x] List customers
- [x] Create customer
- [x] Get customer
- [ ] Update customer
- [ ] Delete customer
- [x] Get customer state

### Orders
- [x] List orders
- [x] Get order
- [ ] Get order invoice

### Subscriptions
- [x] List subscriptions
- [x] Create subscription
- [x] Get subscription
- [ ] Update subscription
- [x] Cancel subscription

### Checkouts (custom)
- [x] Create checkout session
- [x] Get checkout
- [ ] Update checkout
- [ ] Confirm checkout

### Webhooks
- [x] Event parsing
- [ ] Signature verification
- [x] Event type mapping
- [ ] Retry handling

## Configuration Example

```csharp
// appsettings.json used by samples/tests
{
    "PolarSettings": {
        "UseSandbox": true,
        "SandboxApiUrl": "https://sandbox-api.polar.sh",
        "ProductionApiUrl": "https://api.polar.sh",
        "AccessToken": "<SANDBOX_OAT>",
        "OrganizationId": "<ORG_ID>",
        "ProductId": "<PRODUCT_ID>",
        "PriceId": "<PRICE_ID>"
    }
}

// Creating a client
var client = new PolarClient(new PolarClientOptions
{
        AccessToken = accessToken,
        BaseUrl = baseUrl, // e.g., https://sandbox-api.polar.sh
        OrganizationId = organizationId,
        DefaultProductId = productId,
        DefaultPriceId = priceId
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

### Webhook Handling (sample controller excerpt)
```csharp
[HttpPost("webhook")]
public async Task<IActionResult> HandleWebhook()
{
    // Read headers typically sent by Polar
    var signature = Request.Headers["Polar-Webhook-Signature"].FirstOrDefault();
    var eventType = Request.Headers["Polar-Event-Type"].FirstOrDefault();
    var eventId = Request.Headers["Polar-Event-Id"].FirstOrDefault();

    // TODO: Verify signature when WebhookSecret is configured

    using var reader = new StreamReader(Request.Body);
    var body = await reader.ReadToEndAsync();
    var payload = JsonSerializer.Deserialize<PolarWebhookPayload>(body);

    switch (payload?.Type)
    {
        case "customer.created":
            await HandleCustomerCreated(payload.Data);
            break;
        case "subscription.created":
            await HandleSubscriptionCreated(payload.Data);
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
- Package ID: `PolarNet`
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
- **NuGet Package**: https://www.nuget.org/packages/PolarNet

---

*Last Updated: 2025-08-23*
*Version: 1.0.0*