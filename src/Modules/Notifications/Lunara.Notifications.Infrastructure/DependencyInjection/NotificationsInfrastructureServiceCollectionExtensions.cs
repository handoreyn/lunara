using Lunara.Notifications.Application.Ports;
using Lunara.Notifications.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Lunara.Notifications.Infrastructure.DependencyInjection;

/// <summary>
/// Registers Notifications module infrastructure services with the dependency injection container.
/// </summary>
public static class NotificationsInfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Adds EF Core repository implementations for the Notifications module to
    /// <paramref name="services"/>.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same <paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddNotificationsPersistence(
        this IServiceCollection services)
    {
        // Scoped: each request gets its own repository instance sharing the scoped DbContext.
        services.AddScoped<INotificationRepository, EfNotificationRepository>();

        return services;
    }
}
