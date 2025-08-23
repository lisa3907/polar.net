# Polar.NET Test Configuration Guide

## Overview
All tests in this project communicate with the **actual Polar API server** (sandbox environment). There are no mocked or stubbed responses - every test makes real HTTP requests to Polar's API endpoints.

## Prerequisites

### 1. Polar Sandbox Account
- Sign up for a Polar account at https://polar.sh
- Access the sandbox environment at https://sandbox.polar.sh
- Create an organization in the sandbox

### 2. API Access Token
- Navigate to Settings → Access Tokens in your Polar dashboard
- Create a new access token with appropriate permissions
- Copy the token (it starts with `polar_at_` or similar)

### 3. Test Data Setup
In your Polar sandbox account, create:
- At least one Product
- At least one Price for the product
- Note down the IDs for configuration

## Configuration

### appsettings.json
Update the `PolarSettings` section with your actual values:

```json
{
  "PolarSettings": {
    "AccessToken": "polar_at_YOUR_ACTUAL_TOKEN",
    "OrganizationId": "YOUR_ACTUAL_ORG_ID",
    "ProductId": "YOUR_ACTUAL_PRODUCT_ID",
    "PriceId": "YOUR_ACTUAL_PRICE_ID",
    "WebhookBaseUrl": "https://your-webhook-endpoint.ngrok.app",
    "SandboxApiUrl": "https://sandbox-api.polar.sh",
    "ProductionApiUrl": "https://api.polar.sh",
    "UseSandbox": true,
    "WebhookSecret": "YOUR_WEBHOOK_SECRET"
  }
}
```

### Environment Variables (Alternative)
You can also use environment variables with the prefix `POLAR_TEST_`:
- `POLAR_TEST_AccessToken`
- `POLAR_TEST_OrganizationId`
- `POLAR_TEST_ProductId`
- `POLAR_TEST_PriceId`

## Test Architecture

### Real API Communication
- **No Mocking**: All tests use the actual `PolarClient` class
- **Sandbox Environment**: Tests run against `https://sandbox-api.polar.sh`
- **HTTP Requests**: Every test makes real HTTP requests to Polar's API
- **Response Validation**: Tests validate actual API responses

### Test Categories
All tests are organized by API category and inherit from `TestBase`:

| Category | Test File | API Endpoints Tested |
|----------|-----------|---------------------|
| Organization | OrganizationTests.cs | GET /v1/organizations/{id} |
| Products | ProductTests.cs | GET /v1/products, GET /v1/products/{id}, GET /v1/products/{id}/prices |
| Customers | CustomerTests.cs | POST /v1/customers, GET /v1/customers, DELETE /v1/customers/{id} |
| Subscriptions | SubscriptionTests.cs | POST /v1/subscriptions, GET /v1/subscriptions, PATCH /v1/subscriptions/{id} |
| Orders | OrderTests.cs | GET /v1/orders, GET /v1/orders/{id} |
| Checkouts | CheckoutTests.cs | POST /v1/checkouts, GET /v1/checkouts/{id} |
| Payments | PaymentTests.cs | GET /v1/payments, GET /v1/payments/{id} |
| Refunds | RefundTests.cs | POST /v1/refunds, GET /v1/refunds |
| Benefits | BenefitTests.cs | GET /v1/benefits |
| Webhooks | WebhookEndpointTests.cs | Full CRUD for /v1/webhooks/endpoints |

### Test Execution Flow
1. **PolarSandboxFixture** initializes the API client with your credentials
2. **TestBase** provides the client to all test classes
3. Each test method:
   - Checks for valid client configuration (`SkipIfNoClient`)
   - Makes actual API calls using `await Client.MethodAsync()`
   - Validates real API responses
   - Cleans up created resources (e.g., deletes test customers)

## Running Tests

### Prerequisites Check
Before running tests, ensure:
1. ✅ Valid Access Token in appsettings.json
2. ✅ Valid Organization ID
3. ✅ Valid Product ID and Price ID (for checkout/subscription tests)
4. ✅ Internet connection to reach Polar API

### Command Line
```bash
# Run all tests
dotnet test

# Run specific category
dotnet test --filter "FullyQualifiedName~CustomerTests"

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"
```

### Visual Studio / VS Code
- Open Test Explorer
- Run all tests or specific test categories
- Tests will skip with informative messages if configuration is missing

## Test Data Management

### Automatic Cleanup
Tests automatically clean up created resources:
```csharp
var customer = await Client.CreateCustomerAsync(email);
try 
{
    // Test operations
}
finally 
{
    await Client.DeleteCustomerAsync(customer.Id);
}
```

### Test Isolation
- Each test generates unique identifiers (emails, IDs)
- Tests don't depend on specific existing data
- Tests create their own test data when needed

## Troubleshooting

### Tests Skipping
If tests are being skipped:
1. Check that AccessToken is set correctly
2. Verify the token has necessary permissions
3. Ensure UseSandbox is set to `true`
4. Check network connectivity to sandbox-api.polar.sh

### API Errors
Common issues:
- **401 Unauthorized**: Invalid or expired access token
- **403 Forbidden**: Token lacks required permissions
- **404 Not Found**: Invalid IDs in configuration
- **429 Too Many Requests**: Rate limiting - add delays between tests

### Debugging
Enable detailed logging:
```csharp
// In test constructor
Output.WriteLine($"Using API: {Client.BaseUrl}");
Output.WriteLine($"Organization: {OrganizationId}");
```

## Best Practices

1. **Use Sandbox Only**: Never run tests against production
2. **Unique Test Data**: Always generate unique identifiers
3. **Clean Up**: Always delete test resources in finally blocks
4. **Skip Gracefully**: Use Skip.If() for missing configuration
5. **Log Details**: Use Output.WriteLine() for debugging

## Security Notes

- **Never commit real tokens**: Use placeholders in source control
- **Use appsettings.Development.json**: Add to .gitignore
- **Environment Variables**: Preferred for CI/CD environments
- **Token Permissions**: Use minimum required permissions

## CI/CD Integration

For automated testing:
1. Set environment variables in CI/CD pipeline
2. Use a dedicated sandbox account for CI
3. Consider rate limiting and add delays if needed
4. Monitor API usage and costs

---

**Important**: All tests make real API calls. This ensures complete integration testing but requires valid Polar API credentials and an internet connection.