# Polar Sample Project

This is a sample console app demonstrating usage of the Polar .NET SDK.

## Features
- List products and prices
- List orders
- Create, list, and delete customers
- Delete all customers (for test cleanup)

## Prerequisites
- .NET 8.0+ or 9.0 SDK
- A valid Polar API access token

## Setup
1. Copy `appsettings.json` and fill in your credentials:
   ```json
   {
     "PolarSettings": {
       "AccessToken": "<your-access-token>",
       "OrganizationId": "<your-organization-id>",
       "ProductId": "<optional-product-id>",
       "PriceId": "<optional-price-id>",
       "UseSandbox": true,
       "SandboxApiUrl": "https://sandbox-api.polar.sh",
       "ProductionApiUrl": "https://api.polar.sh"
     }
   }
   ```
2. (Optional) Add `appsettings.Development.json` for local overrides.

## Run
```sh
# Windows PowerShell
cd samples/polar.sample
pwsh
# or
# dotnet run --project samples/polar.sample
```

## Usage
After running, select a menu option:
- Run product/order demo: List products and orders
- Delete ALL customers: Remove all test customers
- Create customer: Add a new customer
- List customers: Show all customers
- Delete customer by ID: Remove a specific customer

## Notes
- In DEBUG mode, `appsettings.Development.json` is automatically merged.
- This sample demonstrates usage of key Polar API endpoints.
