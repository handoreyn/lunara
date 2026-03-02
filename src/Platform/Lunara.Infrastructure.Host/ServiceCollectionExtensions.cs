using Lunara.Infrastructure.Host.Messaging;
using Lunara.Infrastructure.Host.Social;
using Lunara.Messaging.Application.UseCases;
using Lunara.Messaging.Infrastructure.DependencyInjection;
using Lunara.Social.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;
using MessagingPorts = Lunara.Messaging.Application.Ports;
using SocialPorts = Lunara.Social.Application.Ports;

namespace Lunara.Infrastructure.Host;

/// <summary>
/// Registers all Lunara module services with the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all Lunara module services to <paramref name="services"/>.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same <paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddLunaraModules(this IServiceCollection services)
    {
        // --- Social module ---
        // Singletons: in-memory stores share state across all requests in a process.
        services.AddSingleton<SocialPorts.ILikeRepository, InMemoryLikeRepository>();
        services.AddSingleton<SocialPorts.IMatchRepository, InMemoryMatchRepository>();

        // Singleton: stateless wall-clock; safe to share.
        services.AddSingleton<SocialPorts.IClock, SystemClock>();

        // Scoped: no-op for now; scoped lifetime matches the EF Core pattern for easy swap-in later.
        services.AddScoped<SocialPorts.IUnitOfWork, InMemoryUnitOfWork>();

        // Scoped: use-case service resolved once per request.
        services.AddScoped<RecordSwipeService>();

        // --- Messaging module ---
        // Singleton: stateless wall-clock; safe to share.
        services.AddSingleton<MessagingPorts.IClock, MessagingSystemClock>();

        // Scoped: EF-backed repositories — registered via the infrastructure extension to keep
        // internal types encapsulated within Messaging.Infrastructure.
        services.AddMessagingPersistence();
        services.AddScoped<MessagingPorts.IUnitOfWork, EfMessagingUnitOfWork>();

        // Singleton: adapts the Social IMatchRepository singleton to satisfy Messaging's read port.
        // Both the Social module (InMemoryMatchRepository) and this adapter are singletons,
        // so they share the same in-memory store.
        services.AddSingleton<MessagingPorts.IMatchReadService, SocialMatchReadService>();

        // Scoped: use-case service resolved once per request.
        services.AddScoped<SendMessageService>();

        return services;
    }
}
