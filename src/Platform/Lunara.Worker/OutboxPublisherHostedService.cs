using Lunara.BuildingBlocks.Outbox;

namespace Lunara.Worker;

/// <summary>
/// Hosted service that periodically polls the transactional outbox and publishes
/// pending messages to Kafka.
/// A new DI scope is created per poll cycle so that the scoped
/// <see cref="IOutboxPoller"/> (and the underlying <c>LunaraDbContext</c>) are
/// properly disposed after each iteration.
/// </summary>
internal sealed partial class OutboxPublisherHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxPublisherHostedService> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(2);

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        LogPublisherStarted(logger);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PollOnceAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
#pragma warning disable CA1031 // background service must not crash on transient errors
            catch (Exception ex)
            {
                LogPollCycleError(logger, ex, PollInterval);
            }
#pragma warning restore CA1031

            try
            {
                await Task.Delay(PollInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        LogPublisherStopped(logger);
    }

    private async Task PollOnceAsync(CancellationToken ct)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        IOutboxPoller poller = scope.ServiceProvider.GetRequiredService<IOutboxPoller>();
        int count = await poller.PollOnceAsync(ct);

        if (count > 0)
        {
            LogPublished(logger, count);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Outbox publisher started.")]
    private static partial void LogPublisherStarted(ILogger logger);

    [LoggerMessage(Level = LogLevel.Error,
        Message = "Unhandled error in outbox publisher poll cycle. Will retry after {Interval}.")]
    private static partial void LogPollCycleError(ILogger logger, Exception ex, TimeSpan interval);

    [LoggerMessage(Level = LogLevel.Information, Message = "Outbox publisher stopped.")]
    private static partial void LogPublisherStopped(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Outbox publisher: published {Count} message(s).")]
    private static partial void LogPublished(ILogger logger, int count);
}
