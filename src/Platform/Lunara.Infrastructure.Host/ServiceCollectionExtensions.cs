using Lunara.BuildingBlocks.Moderation;
using Lunara.Infrastructure.Host.Moderation;
using Lunara.Infrastructure.Host.Social;
using Lunara.Moderation.Application.UseCases;
using Lunara.Social.Application.Ports;
using Lunara.Social.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;
using SocialIClock = Lunara.Social.Application.Ports.IClock;
using BuildingBlocksIClock = Lunara.BuildingBlocks.Clocks.IClock;
using BuildingBlocksSystemClock = Lunara.BuildingBlocks.Clocks.SystemClock;
using ModerationIBlockRepository = Lunara.Moderation.Application.Ports.IBlockRepository;
using ModerationIReportRepository = Lunara.Moderation.Application.Ports.IReportRepository;
using ModerationIUnitOfWork = Lunara.Moderation.Application.Ports.IUnitOfWork;

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
        services.AddSingleton<SocialIClock, SystemClock>();

        // Scoped: no-op for now; scoped lifetime matches the EF Core pattern for easy swap-in later.
        services.AddScoped<IUnitOfWork, InMemoryUnitOfWork>();

        // Scoped: use-case service resolved once per request.
        services.AddScoped<RecordSwipeService>();

        // --- Moderation module ---
        // Shared wall-clock from BuildingBlocks used by Moderation services.
        services.AddSingleton<BuildingBlocksIClock, BuildingBlocksSystemClock>();

        // Scoped: EF Core repositories share the scoped DbContext.
        services.AddScoped<ModerationIBlockRepository, EfBlockRepository>();
        services.AddScoped<ModerationIReportRepository, EfReportRepository>();
        services.AddScoped<ModerationIUnitOfWork, EfModerationUnitOfWork>();

        // Scoped: use-case services resolved once per request.
        services.AddScoped<BlockUserService>();
        services.AddScoped<ReportUserService>();

        // Cross-cutting block checker: adapts IBlockRepository for Social, Messaging, etc.
        services.AddScoped<IBlockChecker, BlockCheckerAdapter>();

        return services;
    }
}
