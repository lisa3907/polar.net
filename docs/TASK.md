# polar.net SDK - Implementation Status

This document tracks the implementation status of the polar.net SDK against the official Polar API Reference (https://github.com/polarsource/polar/tree/main/docs/api-reference).

- Target: Full Polar API coverage
- Source: Current repository source code (src/Services/PolarClient/*, src/Models/*)
- Last Updated: 2025-08-24

## Summary

- Implemented: Checkouts, Subscriptions, Refunds, Payments, Webhook Endpoints, Customers, Products, Orders
- Partially Implemented: Benefits, Organizations
- Not Implemented: Checkout Links, Custom Fields, Customer Meters, Customer Portal, Discounts, Events, Files, License Keys, Meters, Metrics, OAuth2 Connect

---

## Detailed Status

The following mapping follows the directory structure in the Polar API Reference.

### Implemented

- Checkouts
  - Create: `PolarClient.CreateCheckoutAsync(...)`
  - Get: `PolarClient.GetCheckoutAsync(id)`
- Subscriptions
  - Create: `CreateSubscriptionAsync(customerId, priceId?)`
  - List: `ListSubscriptionsAsync(page, limit)`
  - Get: `GetSubscriptionAsync(id)`
  - Cancel: `CancelSubscriptionAsync(id)`
  - Revoke: `RevokeSubscriptionAsync(id)`
- Refunds
  - Create (full/partial): `CreateRefundAsync(...)`, `CreatePartialRefundAsync(...)`
  - List/Get: `ListRefundsAsync(...)`, `GetRefundAsync(id)`
- Payments
  - List/Get: `ListPaymentsAsync(...)`, `GetPaymentAsync(id)`
  - Filters: `ListPaymentsByOrderAsync(orderId, ...)`, `ListPaymentsByCustomerAsync(customerId, ...)`
- Webhooks (Endpoints)
  - CRUD: `CreateWebhookEndpointAsync`, `ListWebhookEndpointsAsync`, `GetWebhookEndpointAsync`, `UpdateWebhookEndpointAsync`, `DeleteWebhookEndpointAsync`
  - Test: `TestWebhookEndpointAsync`
- Customers
  - Create: `CreateCustomerAsync(...)`
  - List/Get/Delete: `ListCustomersAsync(...)`, `GetCustomerAsync(id)`, `DeleteCustomerAsync(id)`
  - State: `GetCustomerStateAsync(id)`
  - Update: `UpdateCustomerAsync(id, UpdateCustomerRequest)`
- Products
  - CRUD: `CreateProductAsync`, `UpdateProductAsync`, `DeleteProductAsync`
  - Read: `GetProductAsync`, `ListProductsAsync`
- Prices
  - Read: `GetPriceAsync`, `ListPricesAsync`
- Orders
  - Read: `ListOrdersAsync`, `GetOrderAsync`
  - Invoices: `GenerateOrderInvoiceAsync(orderId)`, `GetOrderInvoiceAsync(orderId)`

### Partially Implemented

- Benefits
  - Implemented: `ListBenefitsAsync`
  - Missing: Grants and advanced benefit features
- Organizations
  - Implemented: `GetOrganizationAsync`
  - Missing: Additional management endpoints (if any)

### Not Implemented

Per the Polar docs, the following categories currently have no endpoints in the SDK:

- Checkout Links
- Custom Fields
- Customer Meters / Meters
- Customer Portal / Customer Sessions
- Discounts
- Events
- Files (upload/download/list)
- License Keys
- Metrics
- OAuth2 Connect

---

## Immediate Priorities

- Prices: Write operations (if present in API)
  - Implement: `CreatePriceAsync`, `UpdatePriceAsync`, `DeletePriceAsync` in `src/Services/PolarClient/Prices.cs` (or extend `Products.cs` if API groups them)
  - Models: `CreatePriceRequest`, `UpdatePriceRequest` in `src/Models/Requests/`
  - Tests: Add price CRUD tests under `tests/PolarNet.Tests/Categories/`
- Discounts
  - Implement discount CRUD and attachment flows; add models and tests
- Files
  - Implement file upload/download/list; ensure streaming and content-type handling; add tests
- License Keys
  - Implement key create/list/revoke; add models and tests

---

## Implementation Pattern

New endpoints follow existing patterns:

- Service file: `src/Services/PolarClient/{Resource}.cs`
- Resource model: `src/Models/Resources/Polar{Resource}.cs`
- Request model: `src/Models/Requests/{Operation}{Resource}Request.cs`
- Common: Use `PolarClient.SendAsync(HttpMethod, url, HttpContent?)` and `System.Text.Json` for (de)serialization

Example method signatures

```csharp
public async Task<TResource> GetResourceAsync(string id);
public async Task<PolarListResponse<TResource>> ListResourcesAsync(int page = 1, int limit = 10);
public async Task<TResource> CreateResourceAsync(CreateRequest request);
public async Task<TResource> UpdateResourceAsync(string id, UpdateRequest request);
public async Task<bool> DeleteResourceAsync(string id);
```

---

## Quality Gates (for docs)

- Build: ✅
- Lint/Style: n/a
- Tests: Recommended – add category tests when adding new features
- Documentation: XML documentation file generation enabled; major public models now include XML comments

---

## Changelog

- 2025-08-24
  - Converted this document to English to comply with documentation policy
  - Implemented: Customer Update, Product CRUD, Order Invoice (generate/retrieve)
  - Enabled XML documentation build output and added XML comments across key models (payments, refunds, webhooks, invoices, product/customer requests)
  - Listed actionable next steps with file paths