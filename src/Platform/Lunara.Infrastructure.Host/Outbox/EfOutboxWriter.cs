using Lunara.BuildingBlocks.Outbox;
using Lunara.Infrastructure.Host.Persistence;

namespace Lunara.Infrastructure.Host.Outbox;

/// <summary>
/// EF Core implementation of <see cref="IOutboxWriter"/>.
/// Appends a <see cref="OutboxMessageEntity"/> to the current <see cref="LunaraDbContext"/>
/// change-tracker without flushing; the caller's <c>SaveChangesAsync</c> commits the entry
/// atomically with whatever domain state change triggered the event.
/// </summary>
internal sealed class EfOutboxWriter(LunaraDbContext dbContext) : IOutboxWriter
{
    /// <inheritdoc/>
    public Task EnqueueAsync(
        string eventType,
        string payloadJson,
        DateTimeOffset occurredAtUtc,
        string? correlationId,
        CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventType);
        ArgumentException.ThrowIfNullOrWhiteSpace(payloadJson);

        OutboxMessageEntity entry = new()
        {
            Id = Guid.NewGuid(),
            EventType = eventType,
            PayloadJson = payloadJson,
            OccurredAtUtc = occurredAtUtc,
            Status = 0,          // Pending
            LockedUntilUtc = null,
            Attempts = 0,
            LastError = null,
        };

        dbContext.OutboxMessages.Add(entry);

        // No await: EF Core's Add is synchronous; SaveChanges is the caller's responsibility.
        return Task.CompletedTask;
    }
}
