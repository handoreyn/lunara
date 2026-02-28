using Lunara.Infrastructure.Host.Social;
using Lunara.Social.Application.Ports;
using Lunara.Social.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;

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
        services.AddSingleton<ILikeRepository, InMemoryLikeRepository>();
        services.AddSingleton<IMatchRepository, InMemoryMatchRepository>();

        // Singleton: stateless wall-clock; safe to share.
        services.AddSingleton<IClock, SystemClock>();

        // Scoped: no-op for now; scoped lifetime matches the EF Core pattern for easy swap-in later.
        services.AddScoped<IUnitOfWork, InMemoryUnitOfWork>();

        // Scoped: use-case service resolved once per request.
        services.AddScoped<RecordSwipeService>();

        return services;
    }
}
