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

    /// <summary>
    /// Creates a new <see cref="PolarWebhookService"/>.
    /// </summary>
    /// <param name="secret">Webhook signing secret used to compute/verify HMAC signatures. If empty, verification is skipped.</param>
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

        byte[] provided;
        try
        {
            provided = Convert.FromBase64String(signatureBase64);
        }
        catch (FormatException)
        {
            return false;
        }

#if NETSTANDARD2_0
        if (hash.Length != provided.Length) return false;
        var diff = 0;
        for (int i = 0; i < hash.Length; i++) diff |= hash[i] ^ provided[i];
        return diff == 0;
#else
        return CryptographicOperations.FixedTimeEquals(hash, provided);
#endif
    }

    /// <summary>
    /// Parses the JSON webhook envelope into <see cref="PolarWebhookPayload"/>.
    /// </summary>
    /// <param name="rawJson">Raw JSON body as a UTF-8 string.</param>
    /// <returns>Deserialized payload containing the event type and data.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the payload cannot be deserialized.</exception>
    public PolarWebhookPayload Parse(string rawJson)
    {
        var payload = JsonSerializer.Deserialize<PolarWebhookPayload>(rawJson);
        if (payload == null) throw new InvalidOperationException("Invalid webhook payload");
        return payload;
    }

    /// <summary>
    /// Dispatches a webhook to a typed handler based on <see cref="PolarWebhookPayload.Type"/>.
    /// </summary>
    /// <param name="payload">Webhook envelope with <c>type</c> and <c>data</c>.</param>
    /// <param name="handler">Implementation of <see cref="IPolarWebhookEventHandler"/> to receive callbacks.</param>
    /// <remarks>
    /// Supported types: <c>checkout.created</c>, <c>customer.created</c>, <c>order.created</c>, <c>subscription.created</c>.
    /// Unknown types are forwarded to <see cref="IPolarWebhookEventHandler.OnUnknownEvent"/>.
    /// </remarks>
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
