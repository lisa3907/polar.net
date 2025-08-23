# PolarNet.Tests

End-to-end and unit tests for the Polar .NET client. This test project contains:

- Unit tests: fast, isolated tests that do not call Polar APIs.
- Integration tests: live calls against Polar Sandbox or Production (opt-in via configuration). Integration tests are skippable when credentials are not configured.

## What the integration tests cover

Using the sandbox environment, the suite exercises these API flows:

- Organization: Get organization details
- Products: List products, Get product
- Prices: List prices (optionally by product), Get price
- Customers: Create customer, Get customer, List customers, Get customer state
- Subscriptions: Create subscription, Get subscription, List subscriptions, Cancel subscription (cleanup)
- Checkouts: Create custom checkout, Get checkout
- Orders: List orders, Get first order (if any)
- Benefits: List benefits

Additionally, webhook signature verification has unit tests (no network) to validate HMAC logic.

## Configuration

By default, integration tests are skipped unless valid configuration is present. Configuration can be provided via `appsettings.json` in this test project or via environment variables.

appsettings.json (sample)

```
{
  "PolarSettings": {
    "AccessToken": "<YOUR_SANDBOX_OAT>",
    "WebhookSecret": "<YOUR_WEBHOOK_SECRET>",
    "OrganizationId": "<YOUR_ORGANIZATION_ID>",
    "ProductId": "<YOUR_PRODUCT_ID>",
    "PriceId": "<YOUR_PRICE_ID>",
    "SandboxApiUrl": "https://sandbox-api.polar.sh",
    "ProductionApiUrl": "https://api.polar.sh",
    "UseSandbox": true
  }
}
```

Placeholders like `<YOUR_SANDBOX_OAT>` are treated as missing; tests that depend on them will be skipped with a clear message (powered by `Xunit.SkippableFact`).

Environment variables (CI-friendly)

- Use the prefix `POLAR_TEST_` with double-underscore separators to target JSON paths:
  - `POLAR_TEST_PolarSettings__AccessToken`
  - `POLAR_TEST_PolarSettings__OrganizationId`
  - `POLAR_TEST_PolarSettings__ProductId`
  - `POLAR_TEST_PolarSettings__PriceId`
  - `POLAR_TEST_PolarSettings__SandboxApiUrl`
  - `POLAR_TEST_PolarSettings__ProductionApiUrl`
  - `POLAR_TEST_PolarSettings__UseSandbox` (e.g., `true`)

Note: Do not commit secrets. Prefer environment variables in CI.

## How to run

- Run unit tests only (exclude integration):

```powershell
dotnet test .\polar.net.sln --filter "Category!=Integration"
```

- Run integration tests only (requires configuration):

```powershell
dotnet test .\polar.net.sln --filter "Category=Integration"
```

- Run all tests:

```powershell
dotnet test .\polar.net.sln
```

## Skipping behavior

Integration tests use `SkippableFact`. If required configuration is missing or placeholders are detected, tests will Skip with a message rather than fail. This makes the suite safe to run in local/dev environments without credentials.

## Side effects and cleanup

- The subscription test creates a subscription and then calls `CancelSubscriptionAsync` to minimize sandbox pollution.
- Customer records created during tests are not deleted (to keep tests simple). Consider routine cleanup on the sandbox account if needed.

## Dependencies

Key packages used by this test project:

- `xunit`, `xunit.runner.visualstudio`
- `Xunit.SkippableFact` for conditional skipping
- `Microsoft.Extensions.Configuration` (+ JSON + EnvironmentVariables providers)

## Troubleshooting

- 401/403 errors: Check `AccessToken` and the selected environment (`UseSandbox`, `SandboxApiUrl`).
- 404 errors on Get by ID: Verify that `ProductId`/`PriceId` belong to the configured organization.
- Network/timeouts: Ensure outbound access to `https://sandbox-api.polar.sh`.
- Tests skipped unexpectedly: Confirm placeholders were replaced and no leading `<` remains; verify environment variables are spelled correctly with `POLAR_TEST_` prefix.

## Files of interest

- `PolarSandboxTests.cs`: Integration tests and configuration fixture
- `PolarWebhookTests.cs`: Webhook signature unit tests
- `appsettings.json`: Local configuration template
