# Polar Payment Sandbox Test

This is a sandbox web application to test Polar.sh payments and webhooks.

## Getting Started

### 1. Prerequisites

- .NET 9.0 SDK
- Polar Sandbox account (https://sandbox.polar.sh)

### 2. Polar setup

1. Sign up at [Polar Sandbox](https://sandbox.polar.sh)
2. Create or select your Organization
3. Create an Access Token:
   - Go to your Organization page (https://sandbox.polar.sh/[your-org-name])
   - Open the Settings tab
   - In "Access Tokens", click "Create new token"
   - Enter a token name (e.g., "Test API")
   - Select required scopes (e.g., products:read, checkouts:write)
   - Copy the generated token securely (shown only once!)
4. Find your Organization ID:
   - Check the ID in Organization Settings
   - Or infer from URL: https://sandbox.polar.sh/[org-name]
5. Create test products and prices:
   - Create a product in the Products section
   - Add either one-time or recurring prices

### 3. App configuration

Edit the `appsettings.json` file:

```json
{
  "Polar": {
    "AccessToken": "polar_oat_your_sandbox_token_here",
    "WebhookSecret": "whsec_YOUR_WEBHOOK_SECRET_HERE",
    "UseSandbox": true,
    "OrganizationId": "YOUR_ORGANIZATION_ID_HERE"
  }
}
```

### 4. Run

```bash
# Move to project directory
cd tests/polar

# Restore packages
dotnet restore

# Run application
dotnet run
```

When the app starts:
- Main page: https://localhost:7123/polar
- API docs: https://localhost:7123/swagger
- Admin dashboard: https://localhost:7123/polar/admin

## Features

### 1) Payment test
- Show products and prices from Polar
- Create a checkout session for a selected product
- Process payments securely via Stripe
- Verify payment status after completion

### 2) Admin dashboard
- Customer list: view purchasers
- Subscription overview: check active subscriptions
- Stats: see totals for customers, subscriptions, etc.

### 3) Webhook handling
- Real-time events: payments and subscriptions
- Signature verification using StandardWebhooks
- Handles events: checkout, order, subscription, customer

### 4) API endpoints
- `GET /api/polar/products` - list products
- `POST /api/polar/checkout` - create checkout session
- `GET /api/polar/checkout/{id}` - get checkout status
- `GET /api/polar/customers` - list customers
- `GET /api/polar/subscriptions` - list subscriptions
- `POST /api/webhook/polar` - receive webhook


## Test scenarios

### 1) Basic checkout
1. Open https://localhost:7123/polar after starting the app
2. Click "Purchase" on a product
3. You’ll be redirected to the Polar checkout page
4. Enter Stripe test card details:
   - Card: `4242 4242 4242 4242`
   - Expiry: a future date (e.g., 12/25)
   - CVC: any 3 digits (e.g., 123)
5. After payment, you’ll be redirected to the success page

### 2) Manual checkout
1. Use the Manual Checkout Test form
2. Enter a Product Price ID (from Polar dashboard)
3. Optionally enter customer info
4. Click "Create Test Checkout"

### 3) Webhook testing (with ngrok)
```bash
# Install ngrok if needed
# https://ngrok.com/download

# Expose local server with ngrok
ngrok http 7123

# Set webhook URL in the Polar dashboard
# https://your-ngrok-url.ngrok.io/api/webhook/polar
```

## Key code structure

```
tests/polar/
├── Controllers/
│   ├── PolarController.cs      # API endpoints
│   ├── WebhookController.cs    # Webhook handling
│   └── PagesController.cs      # MVC pages
├── Services/
│   └── PolarService.cs         # Polar API integration service
├── Views/Pages/
│   ├── Index.cshtml            # Main page
│   ├── Success.cshtml          # Payment success page
│   ├── Cancel.cshtml           # Payment cancel page
│   └── Admin.cshtml            # Admin dashboard
├── Properties/
│   └── launchSettings.json     # Launch settings
├── appsettings.json            # App settings
├── polar.csproj                # Project file
└── Program.cs                  # Entry point
```

## Troubleshooting

### 1) "401 Unauthorized"
- Verify Access Token
- Check Sandbox/Production environment settings

### 2) Products not visible
- Ensure a product exists in the Polar dashboard
- Verify Organization ID
- Ensure product is not archived

### 3) Webhooks not received
- Use ngrok or similar tunneling tool
- Verify webhook URL in the Polar dashboard
- Verify Webhook Secret

## References

- Polar API: https://docs.polar.sh/api
- Polar Sandbox: https://sandbox.polar.sh
- StandardWebhooks: https://github.com/standard-webhooks/standard-webhooks

## License

This project is provided for testing and learning purposes.

