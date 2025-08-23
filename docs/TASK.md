# API Coverage Analysis: polar.net vs polar-python

## Overview
This document compares the API coverage of the `polar.net` (C#) project with the official Python SDK (`polar-python`, `src/polar_sdk`). It lists which API domains are implemented in `polar.net` and highlights any missing or unimplemented areas.

---

## 1. Core API Domains in polar-python
The following modules represent major API areas in the Python SDK:

- products
- customers
- orders
- subscriptions
- benefits
- organizations
- checkout_links
- checkouts
- payments
- refunds
- files
- events
- webhooks
- discounts
- license_keys
- customer_meters
- metrics
- downloadables
- custom_fields
- customer_sessions
- customer_portal
- oauth2
- ingestion

---

## 2. Implemented in polar.net (PolarClient)
The following API areas are implemented in `PolarClient`:

- Organization: `GetOrganizationAsync`
- Product/Price: `GetProductAsync`, `ListProductsAsync`, `GetPriceAsync`, `ListPricesAsync`
- Customer: `CreateCustomerAsync`, `ListCustomersAsync`, `GetCustomerAsync`, `DeleteCustomerAsync`, `GetCustomerStateAsync`
- Subscription: `CreateSubscriptionAsync`, `ListSubscriptionsAsync`, `GetSubscriptionAsync`, `CancelSubscriptionAsync`, `RevokeSubscriptionAsync`
- Checkout: `CreateCheckoutAsync`, `GetCheckoutAsync`
- Order: `ListOrdersAsync`, `GetOrderAsync`
- Benefit: `ListBenefitsAsync`

---

## 3. Not Yet Implemented in polar.net
The following API areas are present in the Python SDK but not directly implemented in `PolarClient`:

- **payments** (payment operations)
- **refunds** (refund operations)
- **files** (file upload/download)
- **events** (event listing/retrieval)
- **webhooks** (webhook management: create/list/delete; note: webhook receiving/verification is implemented as a service, but not management)
- **discounts** (discount management)
- **license_keys** (license key management)
- **customer_meters**, **metrics**, **downloadables**, **custom_fields**, **customer_sessions**, **customer_portal**, **oauth2**, **ingestion** (various advanced/auxiliary APIs)

---

## 4. Notes
- The core business APIs (products, customers, subscriptions, orders, checkouts, benefits, organizations) are fully implemented in `polar.net`.
- Webhook receiving and signature verification are supported as a service, but webhook management endpoints are not.
- Advanced/auxiliary APIs (files, metrics, discounts, etc.) are not yet implemented in the C# SDK.

---


---

## 6. Priority of Missing APIs
The following prioritization is based on typical SaaS/subscription/payment service needs. Adjust as needed for your business context.

### High Priority
- **payments** (payment processing and status)
- **refunds** (refund processing and status)
- **webhooks** (webhook management: register/list/delete)
- **files** (file upload/download)
- **discounts** (discount/coupon management)
- **license_keys** (license key issuance/validation)

### Medium Priority
- **events** (event log/history)
- **customer_meters**, **metrics** (usage/measurement management)
- **downloadables** (digital asset/resource management)
- **custom_fields** (custom metadata/field management)

### Low Priority
- **customer_sessions**, **customer_portal** (customer portal/session management)
- **oauth2** (OAuth authentication/token management)
- **ingestion** (bulk data ingestion)

---

_Last updated: 2025-08-23_
