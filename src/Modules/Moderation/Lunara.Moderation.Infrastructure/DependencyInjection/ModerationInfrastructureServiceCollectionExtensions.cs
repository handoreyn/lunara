using Lunara.Moderation.Application.Ports;
using Lunara.Moderation.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Lunara.Moderation.Infrastructure.DependencyInjection;

/// <summary>
/// Registers Moderation module infrastructure services with the dependency injection container.
/// </summary>
public static class ModerationInfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Adds EF Core repository implementations for the Moderation module to
    /// <paramref name="services"/>.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same <paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddModerationPersistence(
        this IServiceCollection services)
    {
        // Scoped: each request gets its own repository instance sharing the scoped DbContext.
        services.AddScoped<IBlockRepository, EfBlockRepository>();
        services.AddScoped<IReportRepository, EfReportRepository>();

        return services;
    }
}
