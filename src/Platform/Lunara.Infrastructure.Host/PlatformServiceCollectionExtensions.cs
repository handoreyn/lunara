using Lunara.BuildingBlocks.Clocks;
using Lunara.Infrastructure.Host.Options;
using Lunara.Infrastructure.Host.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lunara.Infrastructure.Host;

/// <summary>
/// Registers cross-cutting platform services (options, clock, database) with the dependency injection container.
/// </summary>
public static class PlatformServiceCollectionExtensions
{
    /// <summary>
    /// Adds platform-level services to <paramref name="services"/>:
    /// <list type="bullet">
    ///   <item><see cref="DatabaseOptions"/> bound from <c>appsettings.json</c>.</item>
    ///   <item><see cref="KafkaOptions"/> bound from <c>appsettings.json</c>.</item>
    ///   <item><see cref="IClock"/> implemented by <see cref="SystemClock"/>.</item>
    ///   <item><see cref="LunaraDbContext"/> using the Npgsql provider.</item>
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

        // Postgres via EF Core — connection string read eagerly so misconfiguration fails fast.
        string connectionString =
            configuration[$"{DatabaseOptions.SectionKey}:ConnectionString"] ?? string.Empty;

        services.AddDbContext<LunaraDbContext>(opts =>
            opts.UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history")));

        return services;
    }
}
