using Lunara.BuildingBlocks.Moderation;
using Lunara.Infrastructure.Host.Messaging;
using Lunara.Infrastructure.Host.Moderation;
using Lunara.Infrastructure.Host.Social;
using Lunara.Messaging.Application.UseCases;
using Lunara.Moderation.Application.UseCases;
using Lunara.Social.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;
using MessagingClock = Lunara.Messaging.Application.Ports.IClock;
using MessagingMessageRepository = Lunara.Messaging.Application.Ports.IMessageRepository;
using MessagingUnitOfWork = Lunara.Messaging.Application.Ports.IUnitOfWork;
using ModerationBlockRepository = Lunara.Moderation.Application.Ports.IBlockRepository;
using ModerationReportRepository = Lunara.Moderation.Application.Ports.IReportRepository;
using ModerationUnitOfWork = Lunara.Moderation.Application.Ports.IUnitOfWork;
using SocialClock = Lunara.Social.Application.Ports.IClock;
using SocialLikeRepository = Lunara.Social.Application.Ports.ILikeRepository;
using SocialMatchRepository = Lunara.Social.Application.Ports.IMatchRepository;
using SocialUnitOfWork = Lunara.Social.Application.Ports.IUnitOfWork;

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
        services.AddSingleton<SocialLikeRepository, InMemoryLikeRepository>();
        services.AddSingleton<SocialMatchRepository, InMemoryMatchRepository>();

        // Singleton: stateless wall-clock; safe to share.
        services.AddSingleton<SocialClock, SystemClock>();

        // Scoped: no-op for now; scoped lifetime matches the EF Core pattern for easy swap-in later.
        services.AddScoped<SocialUnitOfWork, InMemoryUnitOfWork>();

        // Scoped: use-case service resolved once per request.
        services.AddScoped<RecordSwipeService>();

        // --- Moderation module ---
        // Singletons: in-memory stores share state across all requests in a process.
        services.AddSingleton<ModerationBlockRepository, InMemoryBlockRepository>();
        services.AddSingleton<ModerationReportRepository, InMemoryReportRepository>();

        // IBlockChecker bridges the Moderation module to Social and Messaging modules.
        services.AddSingleton<IBlockChecker, InMemoryBlockChecker>();

        // Scoped: no-op UnitOfWork; matches the EF Core pattern for easy swap-in later.
        services.AddScoped<ModerationUnitOfWork, InMemoryModerationUnitOfWork>();

        // Scoped: use-case services resolved once per request.
        services.AddScoped<BlockUserService>();
        services.AddScoped<ReportUserService>();

        // --- Messaging module ---
        services.AddSingleton<MessagingMessageRepository, InMemoryMessageRepository>();
        services.AddSingleton<MessagingClock, MessagingSystemClock>();
        services.AddScoped<MessagingUnitOfWork, InMemoryMessagingUnitOfWork>();
        services.AddScoped<SendMessageService>();

        return services;
    }
}
