namespace Lunara.BuildingBlocks.Inbox;

/// <summary>
/// Provides an idempotency gate for inbound integration events.
/// Implementations record processed event identifiers so that duplicate deliveries
/// from an at-least-once broker are silently discarded.
/// </summary>
public interface IInboxStore
{
    /// <summary>
    /// Atomically inserts an inbox record for <paramref name="eventId"/> / <paramref name="consumer"/>
    /// if one does not already exist.
    /// </summary>
    /// <param name="eventId">The unique identifier of the inbound event.</param>
    /// <param name="consumer">
    ///   Logical name of the consuming module or handler (e.g. <c>"notifications"</c>).
    /// </param>
    /// <param name="nowUtc">Current UTC time, used as the <c>ReceivedAtUtc</c> of the new record.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>
    ///   <see langword="true"/> if the record was inserted (event not yet seen);
    ///   <see langword="false"/> if a record already exists for the same
    ///   <paramref name="eventId"/> / <paramref name="consumer"/> pair (duplicate — caller should skip processing).
    /// </returns>
    Task<bool> TryAcquireAsync(Guid eventId, string consumer, DateTimeOffset nowUtc, CancellationToken ct);

    /// <summary>
    /// Marks an existing inbox record as fully processed, recording the completion timestamp.
    /// Should be called after the consumer has successfully handled the event.
    /// </summary>
    /// <param name="eventId">The unique identifier of the inbound event.</param>
    /// <param name="consumer">
    ///   Logical name of the consuming module or handler (e.g. <c>"notifications"</c>).
    /// </param>
    /// <param name="nowUtc">Current UTC time, used as the processed-at timestamp.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    Task MarkProcessedAsync(Guid eventId, string consumer, DateTimeOffset nowUtc, CancellationToken ct);
}
