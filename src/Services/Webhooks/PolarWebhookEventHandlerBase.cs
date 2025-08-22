using System.Text.Json;
using System.Threading.Tasks;
using PolarNet.Models;

namespace PolarNet.Services;

/// <summary>
/// Base class with no-op implementations for webhook events.
/// Inherit and override only the methods you need.
/// </summary>
public abstract class PolarWebhookEventHandlerBase : IPolarWebhookEventHandler
{
    public virtual Task OnCheckoutCreated(CheckoutCreatedEvent data) => Task.CompletedTask;
    public virtual Task OnCustomerCreated(CustomerCreatedEvent data) => Task.CompletedTask;
    public virtual Task OnOrderCreated(OrderCreatedEvent data) => Task.CompletedTask;
    public virtual Task OnSubscriptionCreated(SubscriptionCreatedEvent data) => Task.CompletedTask;
    public virtual Task OnUnknownEvent(string type, JsonElement data) => Task.CompletedTask;
}
