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
    /// <summary>
    /// Called when a checkout session is created.
    /// Default implementation is a no-op. Override to handle the event.
    /// </summary>
    /// <param name="data">The strongly-typed event payload for the created checkout.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual Task OnCheckoutCreated(CheckoutCreatedEvent data) => Task.CompletedTask;

    /// <summary>
    /// Called when a customer is created.
    /// Default implementation is a no-op. Override to handle the event.
    /// </summary>
    /// <param name="data">The strongly-typed event payload for the created customer.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual Task OnCustomerCreated(CustomerCreatedEvent data) => Task.CompletedTask;

    /// <summary>
    /// Called when an order is created.
    /// Default implementation is a no-op. Override to handle the event.
    /// </summary>
    /// <param name="data">The strongly-typed event payload for the created order.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual Task OnOrderCreated(OrderCreatedEvent data) => Task.CompletedTask;

    /// <summary>
    /// Called when a subscription is created.
    /// Default implementation is a no-op. Override to handle the event.
    /// </summary>
    /// <param name="data">The strongly-typed event payload for the created subscription.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual Task OnSubscriptionCreated(SubscriptionCreatedEvent data) => Task.CompletedTask;

    /// <summary>
    /// Called for webhook events without a dedicated handler method.
    /// Default implementation is a no-op. Override to inspect and handle custom/unknown events.
    /// </summary>
    /// <param name="type">The raw event type string as sent by Polar (e.g., "customer.created").</param>
    /// <param name="data">The raw JSON payload for the event.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual Task OnUnknownEvent(string type, JsonElement data) => Task.CompletedTask;
}
