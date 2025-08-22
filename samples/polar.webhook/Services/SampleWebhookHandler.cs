using System.Text.Json;
using Microsoft.Extensions.Logging;
using PolarNet.Models;
using PolarNet.Services;

namespace Polar.Services;

/// <summary>
/// Sample implementation that logs webhook events. Replace with your app logic.
/// </summary>
public sealed class SampleWebhookHandler : PolarWebhookEventHandlerBase
{
    private readonly ILogger<SampleWebhookHandler> _logger;
    public SampleWebhookHandler(ILogger<SampleWebhookHandler> logger) => _logger = logger;

    public override Task OnCheckoutCreated(CheckoutCreatedEvent data)
    {
        _logger.LogInformation("[Handler] checkout.created: {Id} status={Status}", data.Id, data.Status);
        return Task.CompletedTask;
    }

    public override Task OnCustomerCreated(CustomerCreatedEvent data)
    {
        _logger.LogInformation("[Handler] customer.created: {Id} email={Email}", data.Id, data.Email);
        return Task.CompletedTask;
    }

    public override Task OnOrderCreated(OrderCreatedEvent data)
    {
        _logger.LogInformation("[Handler] order.created: {Id} amount={Amount} {Currency}", data.Id, data.Amount, data.Currency);
        return Task.CompletedTask;
    }

    public override Task OnSubscriptionCreated(SubscriptionCreatedEvent data)
    {
        _logger.LogInformation("[Handler] subscription.created: {Id} customer={CustomerId}", data.Id, data.CustomerId);
        return Task.CompletedTask;
    }

    public override Task OnUnknownEvent(string type, JsonElement data)
    {
        _logger.LogInformation("[Handler] unknown event: {Type}", type);
        return Task.CompletedTask;
    }
}
