using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PolarNet.Models;

namespace PolarNet.Services;

/// <summary>
/// Verifies Polar webhook signatures, parses the payload envelope, and dispatches to a handler.
/// </summary>
public sealed class PolarWebhookService
{
    private readonly string _secret;

    public PolarWebhookService(string secret)
    {
        _secret = secret ?? string.Empty;
    }

    /// <summary>
    /// Verifies the webhook signature using HMACSHA256 over the raw body.
    /// When secret is empty, returns true (no verification in dev).
    /// </summary>
    public bool VerifySignature(byte[] rawBody, string signatureBase64)
    {
        if (string.IsNullOrWhiteSpace(_secret)) return true;
        if (string.IsNullOrWhiteSpace(signatureBase64)) return false;

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_secret));
        var hash = hmac.ComputeHash(rawBody);
        var computed = Convert.ToBase64String(hash);

        // Fixed-time comparison fallback for older TFMs
        var a = Encoding.UTF8.GetBytes(computed);
        var b = Encoding.UTF8.GetBytes(signatureBase64);
        if (a.Length != b.Length) return false;
        var diff = 0;
        for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
        return diff == 0;
    }

    public PolarWebhookPayload Parse(string rawJson)
    {
        var payload = JsonSerializer.Deserialize<PolarWebhookPayload>(rawJson);
        if (payload == null) throw new InvalidOperationException("Invalid webhook payload");
        return payload;
    }

    public async Task DispatchAsync(PolarWebhookPayload payload, IPolarWebhookEventHandler handler)
    {
        switch (payload.Type)
        {
            case "checkout.created":
            {
                var data = payload.Data.Deserialize<CheckoutCreatedEvent>();
                if (data != null) { await handler.OnCheckoutCreated(data); return; }
                break;
            }
            case "customer.created":
            {
                var data = payload.Data.Deserialize<CustomerCreatedEvent>();
                if (data != null) { await handler.OnCustomerCreated(data); return; }
                break;
            }
            case "order.created":
            {
                var data = payload.Data.Deserialize<OrderCreatedEvent>();
                if (data != null) { await handler.OnOrderCreated(data); return; }
                break;
            }
            case "subscription.created":
            {
                var data = payload.Data.Deserialize<SubscriptionCreatedEvent>();
                if (data != null) { await handler.OnSubscriptionCreated(data); return; }
                break;
            }
        }

        await handler.OnUnknownEvent(payload.Type, payload.Data);
    }
}
