using Lunara.BuildingBlocks.Clocks;
using Lunara.Infrastructure.Host.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lunara.Infrastructure.Host;

/// <summary>
/// Registers cross-cutting platform services (options, clock) with the dependency injection container.
/// </summary>
public static class PlatformServiceCollectionExtensions
{
    /// <summary>
    /// Adds platform-level services to <paramref name="services"/>:
    /// <list type="bullet">
    ///   <item><see cref="DatabaseOptions"/> bound from <c>appsettings.json</c>.</item>
    ///   <item><see cref="KafkaOptions"/> bound from <c>appsettings.json</c>.</item>
    ///   <item><see cref="IClock"/> implemented by <see cref="SystemClock"/>.</item>
    /// </list>
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration root.</param>
    /// <returns>The same <paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddLunaraPlatform(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionKey));
        services.Configure<KafkaOptions>(configuration.GetSection(KafkaOptions.SectionKey));

        // Singleton: stateless wall-clock implementation.
        services.AddSingleton<IClock, SystemClock>();

        return services;
    }
}
