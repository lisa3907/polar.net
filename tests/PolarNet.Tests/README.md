# PolarNet.Tests

End-to-end and unit tests for the Polar .NET client. This test project contains:

- Unit tests: fast, isolated tests that do not call Polar APIs.
- Integration tests: **REAL API COMMUNICATION** - All tests make actual HTTP requests to Polar's sandbox API server. No mocking or stubbing is used.

**⚠️ IMPORTANT**: All integration tests communicate with the actual Polar API server. See [TEST_CONFIGURATION.md](TEST_CONFIGURATION.md) for detailed setup instructions.

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

Additionally, webhook signature verification has unit tests (no network) to validate HMAC logic, and an optional Sandbox end-to-end (E2E) webhook test verifies real delivery via the sample webhook server.

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
  "UseSandbox": true,
  "WebhookBaseUrl": "<YOUR_PUBLIC_WEBHOOK_BASE_URL>"
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
  - `POLAR_TEST_PolarSettings__WebhookBaseUrl` (e.g., `https://abc123.ngrok-free.app`)

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

## Webhook E2E testing (Sandbox, real delivery)

Follow the steps below to run an end-to-end webhook test (`Webhook_Receives_CustomerCreated_Event`) against the Sandbox. If required settings are missing, the test will be skipped automatically.

Prerequisites
- Sample webhook server: `samples/polar.webhook`
- Public URL: expose your local server via ngrok (or similar)
- Polar Sandbox account and Sandbox OAT access token

1) Run the sample webhook server and expose it publicly
- After starting the sample app, obtain a public URL.
  - Receive endpoint: `POST /api/webhook/polar`
  - Test/list endpoints: `GET /api/webhook/test`, `GET /api/webhook/events`
- ngrok example: expose local port 5000 and use the issued HTTPS URL (e.g., `https://abc123.ngrok-free.app`).

2) Register the webhook in Polar Sandbox
- Polar Sandbox → Settings → Webhooks → Add Webhook
  - Endpoint URL: `<ngrok>/api/webhook/polar`
  - Secret: optional. If you set a secret, also set the same value in the sample app under `PolarSettings:WebhookSecret` to pass signature verification.
  - Events: include `customer.created`

3) Update test settings
- Configure via `tests/PolarNet.Tests/appsettings.json` or environment variables:
  - `PolarSettings:AccessToken` = Sandbox OAT
  - `PolarSettings:UseSandbox` = true
  - `PolarSettings:SandboxApiUrl` = `https://sandbox-api.polar.sh`
  - `PolarSettings:WebhookBaseUrl` = your ngrok HTTPS URL (e.g., `https://abc123.ngrok-free.app`)

4) Run the integration tests
- Run all integration tests
```powershell
dotnet test .\tests\PolarNet.Tests\PolarNet.Tests.csproj --filter "Category=Integration"
```
- Run only the webhook E2E test (optional)
```powershell
dotnet test .\tests\PolarNet.Tests\PolarNet.Tests.csproj --filter "FullyQualifiedName~Webhook_Receives_CustomerCreated_Event"
```

How it works
- The test creates a customer in the Polar Sandbox to trigger a `customer.created` event.
- It then polls the sample server’s `GET /api/webhook/events` endpoint to verify receipt (up to 2 minutes).
- If the event is observed, the test passes; if not (due to URL/network issues), it fails on timeout.

Tips
- Ensure ngrok isn’t blocked by your firewall/proxy.
- If a secret is configured, update the sample app’s `appsettings.json` with the same secret.
- Because this relies on external networking, occasional delays can happen; the test includes retries.
