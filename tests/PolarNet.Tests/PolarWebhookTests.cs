using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PolarNet.Models;
using PolarNet.Services;
using Xunit;

namespace PolarNet.Tests
{
    public class PolarWebhookTests
    {
        private static string ComputeSignature(string secret, byte[] body)
        {
            using var h = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            return Convert.ToBase64String(h.ComputeHash(body));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void VerifySignature_Allows_When_Secret_Empty()
        {
            var svc = new PolarWebhookService(secret: "");
            var ok = svc.VerifySignature(Encoding.UTF8.GetBytes("{}"), signatureBase64: "");
            Assert.True(ok);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void VerifySignature_Computes_HmacSha256_Base64()
        {
            var secret = "topsecret";
            var payload = Encoding.UTF8.GetBytes("{\"hello\":\"world\"}");
            var expected = ComputeSignature(secret, payload);

            var svc = new PolarWebhookService(secret);
            Assert.True(svc.VerifySignature(payload, expected));
            Assert.False(svc.VerifySignature(payload, expected + "bad"));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Parse_Envelope_Roundtrips()
        {
            var now = DateTime.UtcNow;
            var json = JsonSerializer.Serialize(new
            {
                type = "customer.created",
                data = new { id = "cus_123", email = "a@b.com", name = "A", organization_id = "org_1", created_at = now },
                event_id = "evt_1",
                created_at = now
            });

            var svc = new PolarWebhookService("irrelevant");
            var payload = svc.Parse(json);

            Assert.Equal("customer.created", payload.Type);
            Assert.Equal("evt_1", payload.EventId);
            Assert.True(payload.Data.ValueKind == JsonValueKind.Object);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Dispatch_Invokes_Strongly_Typed_Handlers()
        {
            var now = DateTime.UtcNow;
            string MakeJson(string type, object data) => JsonSerializer.Serialize(new { type, data, event_id = "evt_x", created_at = now });

            // checkout.created
            var checkoutJson = MakeJson("checkout.created", new { id = "chk_1", status = "created", customer_id = "", product_id = "prod_1", success_url = "https://x", created_at = now });
            var customerJson = MakeJson("customer.created", new { id = "cus_1", email = "e@x.com", name = "N", organization_id = "org_1", created_at = now });
            var orderJson = MakeJson("order.created", new { id = "ord_1", customer_id = "cus_1", product_id = "prod_1", amount = 1000, currency = "USD", status = "paid", created_at = now });
            var subJson = MakeJson("subscription.created", new { id = "sub_1", status = "active", customer_id = "cus_1", product_id = "prod_1", price_id = "price_1", created_at = now });

            var svc = new PolarWebhookService("irrelevant");
            var handler = new CapturingHandler();

            await svc.DispatchAsync(svc.Parse(checkoutJson), handler);
            Assert.Equal("chk_1", handler.CapturedCheckout?.Id);

            await svc.DispatchAsync(svc.Parse(customerJson), handler);
            Assert.Equal("cus_1", handler.CapturedCustomer?.Id);

            await svc.DispatchAsync(svc.Parse(orderJson), handler);
            Assert.Equal("ord_1", handler.CapturedOrder?.Id);

            await svc.DispatchAsync(svc.Parse(subJson), handler);
            Assert.Equal("sub_1", handler.CapturedSubscription?.Id);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Dispatch_Unknown_Event_Falls_Back()
        {
            var now = DateTime.UtcNow;
            var json = JsonSerializer.Serialize(new { type = "unknown.type", data = new { foo = 1 }, event_id = "evt_2", created_at = now });

            var svc = new PolarWebhookService("irrelevant");
            var handler = new CapturingHandler();
            await svc.DispatchAsync(svc.Parse(json), handler);

            Assert.Equal("unknown.type", handler.UnknownType);
            Assert.True(handler.UnknownData.HasValue);
        }

        private sealed class CapturingHandler : PolarWebhookEventHandlerBase
        {
            public CheckoutCreatedEvent? CapturedCheckout { get; private set; }
            public CustomerCreatedEvent? CapturedCustomer { get; private set; }
            public OrderCreatedEvent? CapturedOrder { get; private set; }
            public SubscriptionCreatedEvent? CapturedSubscription { get; private set; }
            public string? UnknownType { get; private set; }
            public JsonElement? UnknownData { get; private set; }

            public override Task OnCheckoutCreated(CheckoutCreatedEvent data)
            {
                CapturedCheckout = data; return Task.CompletedTask;
            }

            public override Task OnCustomerCreated(CustomerCreatedEvent data)
            {
                CapturedCustomer = data; return Task.CompletedTask;
            }

            public override Task OnOrderCreated(OrderCreatedEvent data)
            {
                CapturedOrder = data; return Task.CompletedTask;
            }

            public override Task OnSubscriptionCreated(SubscriptionCreatedEvent data)
            {
                CapturedSubscription = data; return Task.CompletedTask;
            }

            public override Task OnUnknownEvent(string type, JsonElement data)
            {
                UnknownType = type; UnknownData = data; return Task.CompletedTask;
            }
        }
    }
}
// Removed: webhook unit tests. Only sandbox integration tests remain.
