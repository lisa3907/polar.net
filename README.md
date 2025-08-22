# Polar.NET

[![NuGet](https://img.shields.io/nuget/v/Polar.Net.svg)](https://www.nuget.org/packages/Polar.Net/)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0-blue.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE.md)
[![Downloads](https://img.shields.io/nuget/dt/Polar.Net.svg)](https://www.nuget.org/packages/Polar.Net/)

A C# console application to test Polar.sh payment platform Sandbox API.

## Project structure

```
polar.net/
├── src/
│   ├── Models/
│   │   └── PolarModels.cs
│   ├── Services/
│   │   └── PolarSandboxAPI.cs
│   ├── Program.cs
│   ├── appsettings.json
│   └── polar.net.csproj
└── README.md
```

## Setup

### 1) Restore

```bash
git clone https://github.com/lisa3907/polar.net.git
cd polar.net\src
dotnet restore
```

### 2) Configure settings

Edit `src/appsettings.json` and set:

- AccessToken: Access token generated in Polar Sandbox
- OrganizationId: Your organization ID
- ProductId: A product ID to test with
- PriceId: A price ID (e.g., a free recurring price)

### 3) Run

```bash
cd src
dotnet run
```

## Features

1. View organization info
2. View product info (including prices)
3. Manage customers (create and list)
4. Manage subscriptions (create, list, cancel)
5. Create checkout session
6. Summary of all data (counts)
7. Automated test flow

## API endpoints

- Organization: `/v1/organizations`
- Products: `/v1/products`
- Customers: `/v1/customers`
- Subscriptions: `/v1/subscriptions`
- Checkouts: `/v1/checkouts/custom`
- Orders: `/v1/orders`
- Benefits: `/v1/benefits`

## Test card info

Stripe test cards useful in Sandbox when testing paid products:

- Success: 4242 4242 4242 4242
- Failure: 4000 0000 0000 0002
- 3D Secure: 4000 0025 0000 3155

## Troubleshooting

### 401 Unauthorized
- Verify token is correct
- Ensure it’s a Sandbox token

### 404 Not Found
- Verify IDs
- Ensure data exists in Sandbox environment

### 422 Unprocessable Entity
- Verify request payload shape
- Check required fields are present

## License

MIT License

## References

- Polar Documentation: https://docs.polar.sh
- Polar Sandbox: https://sandbox.polar.sh
- Polar API Reference: https://docs.polar.sh/api-reference
- Polar: https://polar.sh
