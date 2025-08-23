using System.Text.Json;
using System.Threading.Tasks;
using PolarNet.Models;

namespace PolarNet.Services;

/// <summary>
/// Contract for handling Polar webhook events.
/// </summary>
public interface IPolarWebhookEventHandler
{
    /// <summary>
    /// Called when a <c>checkout.created</c> event is received.
    /// </summary>
    /// <param name="data">Event payload specific to the checkout.</param>
    Task OnCheckoutCreated(CheckoutCreatedEvent data);

    /// <summary>
    /// Called when a <c>customer.created</c> event is received.
    /// </summary>
    /// <param name="data">Event payload specific to the customer.</param>
    Task OnCustomerCreated(CustomerCreatedEvent data);

    /// <summary>
    /// Called when an <c>order.created</c> event is received.
    /// </summary>
    /// <param name="data">Event payload specific to the order.</param>
    Task OnOrderCreated(OrderCreatedEvent data);

    /// <summary>
    /// Called when a <c>subscription.created</c> event is received.
    /// </summary>
    /// <param name="data">Event payload specific to the subscription.</param>
    Task OnSubscriptionCreated(SubscriptionCreatedEvent data);

    /// <summary>
    /// Called for unrecognized event types.
    /// </summary>
    /// <param name="type">Raw event type string.</param>
    /// <param name="data">Event payload as JSON.</param>
    Task OnUnknownEvent(string type, JsonElement data);
}
