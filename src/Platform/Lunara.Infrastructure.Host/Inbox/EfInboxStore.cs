using Lunara.BuildingBlocks.Inbox;
using Lunara.Infrastructure.Host.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Lunara.Infrastructure.Host.Inbox;

/// <summary>
/// EF Core implementation of <see cref="IInboxStore"/> backed by <see cref="LunaraDbContext"/>.
/// Uses a unique constraint on <c>(Consumer, EventId)</c> as the concurrency-safe idempotency gate.
/// </summary>
internal sealed class EfInboxStore(LunaraDbContext dbContext) : IInboxStore
{
    // Postgres error code for unique constraint violations.
    private const string UniqueViolationSqlState = "23505";

    /// <inheritdoc/>
    /// <remarks>
    /// Attempts to insert a new <see cref="InboxMessageEntity"/> row within the current scope.
    /// If the unique constraint on <c>(Consumer, EventId)</c> fires, the exception is caught and
    /// <see langword="false"/> is returned — signalling to the caller that this event was already
    /// received and must not be processed again.
    /// </remarks>
    public async Task<bool> TryAcquireAsync(
        Guid eventId,
        string consumer,
        DateTimeOffset nowUtc,
        CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(consumer);

        InboxMessageEntity entry = new()
        {
            EventId = eventId,
            Consumer = consumer,
            ReceivedAtUtc = nowUtc,
            ProcessedAtUtc = null,
        };

        dbContext.InboxMessages.Add(entry);

        try
        {
            await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
            return true;
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException pg
               && pg.SqlState == UniqueViolationSqlState)
        {
            // Row already exists — duplicate delivery. Detach the entry so the context
            // remains usable for subsequent operations in the same scope.
            dbContext.Entry(entry).State = EntityState.Detached;
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task MarkProcessedAsync(
        Guid eventId,
        string consumer,
        DateTimeOffset nowUtc,
        CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(consumer);

        await dbContext.InboxMessages
            .Where(m => m.EventId == eventId && m.Consumer == consumer)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(m => m.ProcessedAtUtc, nowUtc),
                ct)
            .ConfigureAwait(false);
    }
}
