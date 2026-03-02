using Lunara.Messaging.Application.Ports;
using Lunara.Messaging.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Lunara.Messaging.Infrastructure.DependencyInjection;

/// <summary>
/// Registers Messaging module infrastructure services with the dependency injection container.
/// </summary>
public static class MessagingInfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Adds EF Core repository implementations for the Messaging module to
    /// <paramref name="services"/>.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same <paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddMessagingPersistence(
        this IServiceCollection services)
    {
        // Scoped: each request gets its own repository instance sharing the scoped DbContext.
        services.AddScoped<IConversationRepository, EfConversationRepository>();
        services.AddScoped<IMessageRepository, EfMessageRepository>();

        return services;
    }
}
