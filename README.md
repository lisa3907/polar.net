# PolarNet

[![NuGet](https://img.shields.io/nuget/v/PolarNet.svg)](https://www.nuget.org/packages/PolarNet/)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0-blue.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE.md)
[![Downloads](https://img.shields.io/nuget/dt/PolarNet.svg)](https://www.nuget.org/packages/PolarNet/)

 Thin C# client library for Polar API with samples (console + ASP.NET webhook).

 Supported frameworks: netstandard2.0, netstandard2.1, .NET 8, .NET 9

## Project structure

```
polar.net/
├── src/                      # Class library (packable)
│   ├── Models/               # Typed API models (split per class)
│   ├── Services/             # Low-level HTTP service
│   ├── PolarClient.cs        # Public client facade
│   ├── PolarClientOptions.cs # Client options
│   └── polar.net.csproj
├── samples/
│   ├── polar.sample/         # Console app demonstrating API calls
│   └── polar.webhook/        # ASP.NET webhook receiver sample
├── tests/                    # xUnit tests
└── README.md
```

## Quick start

1) Clone & restore

```powershell
git clone https://github.com/lisa3907/polar.net.git
cd polar.net
dotnet restore
```

2) Build the solution

```powershell
dotnet build -c Debug
```

3) Run the console sample (dotnet run uses the sample project)

```powershell
cd samples/polar.sample
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
		"UseSandbox": true,
		"SandboxApiUrl": "https://sandbox-api.polar.sh",
		"ProductionApiUrl": "https://api.polar.sh",
		"AccessToken": "<SANDBOX_OAT>",
		"OrganizationId": "<ORG_ID>",
		"ProductId": "<PRODUCT_ID>",
		"PriceId": "<PRICE_ID>"
	}
}
```

## Using the library (programmatic)

Add a project reference to `src/polar.net.csproj` (already wired for samples), then:

```csharp
var client = new PolarNet.PolarClient(new PolarNet.PolarClientOptions
{
	AccessToken = "<SANDBOX_OAT>",
	BaseUrl = "https://sandbox-api.polar.sh", // no trailing slash
	OrganizationId = "<ORG_ID>",
	DefaultProductId = "<PRODUCT_ID>",
	DefaultPriceId = "<PRICE_ID>"
});

var org = await client.GetOrganizationAsync();
```

## Implemented endpoints (current)

- Organization: GET `/v1/organizations/{organization_id}`
- Products: GET `/v1/products?organization_id={org}` · GET `/v1/products/{id}`
- Prices: GET `/v1/prices?organization_id={org}[&product_id={pid}]` · GET `/v1/prices/{id}`
- Customers: POST `/v1/customers` · GET `/v1/customers?organization_id={org}` · GET `/v1/customers/{id}` · GET `/v1/customers/{id}/state`
- Subscriptions: POST `/v1/subscriptions` · GET `/v1/subscriptions?organization_id={org}` · GET `/v1/subscriptions/{id}` · DELETE `/v1/subscriptions/{id}`
- Checkouts (custom): POST `/v1/checkouts/custom` · GET `/v1/checkouts/custom/{id}`
- Orders: GET `/v1/orders?organization_id={org}` · GET `/v1/orders/{id}`
- Benefits: GET `/v1/benefits?organization_id={org}`

## Test card info

Stripe test cards useful in Sandbox when testing paid products:

- Success: 4242 4242 4242 4242
- Failure: 4000 0000 0000 0002
- 3D Secure: 4000 0025 0000 3155

## Troubleshooting

- 401 Unauthorized: Verify the token is sandbox, valid scopes, and not expired.
- 404 Not Found: Verify IDs exist in your sandbox.
- 422 Unprocessable Entity: Check request payload and required fields.

## Configuration notes

- Authentication uses a Bearer Organization Access Token in the `Authorization` header.
- Set `BaseUrl` to the host (sandbox: `https://sandbox-api.polar.sh`, production: `https://api.polar.sh`).
- Some helper methods use defaults from options (`OrganizationId`, `DefaultProductId`, `DefaultPriceId`).

## License

MIT License — see [LICENSE.md](LICENSE.md)

## References

- Polar Documentation: https://docs.polar.sh
- Polar Sandbox: https://sandbox.polar.sh
- Polar API Reference: https://docs.polar.sh/api-reference
- Polar: https://polar.sh


## 👥 Team

### **Core Development Team**
- **SEONGAHN** - Lead Developer & Project Architect ([lisa@odinsoft.co.kr](mailto:lisa@odinsoft.co.kr))
- **YUJIN** - Senior Developer & Exchange Integration Specialist ([yoojin@odinsoft.co.kr](mailto:yoojin@odinsoft.co.kr))
- **SEJIN** - Software Developer & API Implementation ([saejin@odinsoft.co.kr](mailto:saejin@odinsoft.co.kr))

## 📞 Support & Contact

- **🐛 Issues**: [GitHub Issues](https://github.com/lisa3907/polar.net/issues)
- **📧 Email**: help@odinsoft.co.kr

## 📄 License

MIT License - see [LICENSE.md](LICENSE.md) for details.

---

**Built with ❤️ by the ODINSOFT Team** | [⭐ Star us on GitHub](https://github.com/lisa3907/polar.net)