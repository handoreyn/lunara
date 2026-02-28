using Lunara.BuildingBlocks.Clocks;
using Lunara.BuildingBlocks.Kafka;
using Lunara.BuildingBlocks.Outbox;
using Lunara.Infrastructure.Host.Options;
using Lunara.Infrastructure.Host.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lunara.Infrastructure.Host.Outbox;

/// <summary>
/// EF Core implementation of <see cref="IOutboxPoller"/>.
/// Queries pending outbox messages, locks them, publishes each to Kafka,
/// then records the outcome back to the database.
/// </summary>
internal sealed partial class EfOutboxPoller(
    LunaraDbContext dbContext,
    IKafkaProducer kafkaProducer,
    IOptions<KafkaOptions> kafkaOptions,
    IClock clock,
    ILogger<EfOutboxPoller> logger) : IOutboxPoller
{
    private const int BatchSize = 20;
    private const int LockDurationSeconds = 30;
    private const int MaxAttempts = 5;

    /// <inheritdoc/>
    public async Task<int> PollOnceAsync(CancellationToken ct)
    {
        DateTimeOffset now = clock.UtcNow;

        List<OutboxMessageEntity> messages = await dbContext.OutboxMessages
            .Where(m =>
                m.Status == OutboxMessageStatus.Pending
                && (m.LockedUntilUtc == null || m.LockedUntilUtc < now))
            .OrderBy(m => m.OccurredAtUtc)
            .Take(BatchSize)
            .ToListAsync(ct);

        if (messages.Count == 0)
        {
            return 0;
        }

        // Acquire advisory locks to prevent concurrent processors from picking the same rows.
        DateTimeOffset lockUntil = now.AddSeconds(LockDurationSeconds);

        foreach (OutboxMessageEntity message in messages)
        {
            message.LockedUntilUtc = lockUntil;
        }

        await dbContext.SaveChangesAsync(ct);

        // Publish each locked message and record outcomes.
        foreach (OutboxMessageEntity message in messages)
        {
            await PublishMessageAsync(message, ct);
        }

        await dbContext.SaveChangesAsync(ct);

        return messages.Count;
    }

    private async Task PublishMessageAsync(OutboxMessageEntity message, CancellationToken ct)
    {
        string topic = $"{kafkaOptions.Value.TopicPrefix}{message.EventType}";

        try
        {
            await kafkaProducer.PublishAsync(
                topic,
                message.Id.ToString(),
                message.PayloadJson,
                headers: null,
                ct);

            message.Status = OutboxMessageStatus.Sent;
            message.LockedUntilUtc = null;

            LogMessagePublished(logger, message.Id, message.EventType, topic);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            message.Attempts++;
            message.LastError = ex.Message;
            message.LockedUntilUtc = null;

            if (message.Attempts >= MaxAttempts)
            {
                message.Status = OutboxMessageStatus.Failed;

                LogMessageFailed(logger, ex, message.Id, message.EventType, MaxAttempts);
            }
            else
            {
                LogMessageRetry(logger, ex, message.Id, message.EventType, message.Attempts, MaxAttempts);
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Debug,
        Message = "Outbox message {Id} ({EventType}) published to topic '{Topic}'.")]
    private static partial void LogMessagePublished(ILogger logger, Guid id, string eventType, string topic);

    [LoggerMessage(Level = LogLevel.Error,
        Message = "Outbox message {Id} ({EventType}) exhausted {Max} attempts. Marking as failed.")]
    private static partial void LogMessageFailed(ILogger logger, Exception ex, Guid id, string eventType, int max);

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "Outbox message {Id} ({EventType}) failed (attempt {Attempt}/{Max}). Will retry.")]
    private static partial void LogMessageRetry(ILogger logger, Exception ex, Guid id, string eventType, int attempt, int max);
}
