using System.Text.Json;
using System.Threading.Tasks;
using PolarNet.Models;

namespace PolarNet.Services;

/// <summary>
/// Contract for handling Polar webhook events.
/// </summary>
public interface IPolarWebhookEventHandler
{
    Task OnCheckoutCreated(CheckoutCreatedEvent data);
    Task OnCustomerCreated(CustomerCreatedEvent data);
    Task OnOrderCreated(OrderCreatedEvent data);
    Task OnSubscriptionCreated(SubscriptionCreatedEvent data);
    Task OnUnknownEvent(string type, JsonElement data);
}
