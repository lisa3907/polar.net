// Enable nullable to allow string? annotations in this file only
#nullable enable

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PolarNet.Services;

namespace PolarNet.Extensions;

/// <summary>
/// DI extensions for PolarNet services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers Polar webhook services with a static secret.
    /// </summary>
    public static IServiceCollection AddPolarWebhooks(this IServiceCollection services, string? secret)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        services.AddSingleton(new PolarWebhookService(secret ?? string.Empty));
        return services;
    }

    /// <summary>
    /// Registers Polar webhook services reading secret from configuration path (default: "PolarSettings:WebhookSecret").
    /// </summary>
    public static IServiceCollection AddPolarWebhooks(this IServiceCollection services, IConfiguration configuration, string secretKeyPath = "PolarSettings:WebhookSecret")
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));
        var secret = configuration[secretKeyPath];
        services.AddSingleton(new PolarWebhookService(secret ?? string.Empty));
        return services;
    }

    /// <summary>
    /// Registers Polar webhook services and a concrete handler type.
    /// </summary>
    public static IServiceCollection AddPolarWebhooks<THandler>(this IServiceCollection services, string? secret)
        where THandler : class, IPolarWebhookEventHandler
    {
        services.AddPolarWebhooks(secret);
        services.AddSingleton<IPolarWebhookEventHandler, THandler>();
        return services;
    }

    /// <summary>
    /// Registers Polar webhook services and a concrete handler type using configuration for the secret.
    /// </summary>
    public static IServiceCollection AddPolarWebhooks<THandler>(this IServiceCollection services, IConfiguration configuration, string secretKeyPath = "PolarSettings:WebhookSecret")
        where THandler : class, IPolarWebhookEventHandler
    {
        services.AddPolarWebhooks(configuration, secretKeyPath);
        services.AddSingleton<IPolarWebhookEventHandler, THandler>();
        return services;
    }
}
