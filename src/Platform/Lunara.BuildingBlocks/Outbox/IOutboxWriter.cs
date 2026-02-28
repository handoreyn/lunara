namespace Lunara.BuildingBlocks.Outbox;

/// <summary>
/// Enqueues integration events into the transactional outbox for reliable,
/// at-least-once delivery to the message broker.
/// </summary>
public interface IOutboxWriter
{
    /// <summary>
    /// Appends a new pending outbox entry within the current ambient transaction.
    /// The caller is responsible for calling <c>SaveChangesAsync</c> (or equivalent)
    /// on the same unit-of-work to commit the entry atomically with the domain state change.
    /// </summary>
    /// <param name="eventType">
    ///   Short, dot-separated event type identifier (e.g. <c>"social.match.created"</c>).
    /// </param>
    /// <param name="payloadJson">The event payload serialised as a JSON string.</param>
    /// <param name="occurredAtUtc">UTC timestamp of when the domain event occurred.</param>
    /// <param name="correlationId">
    ///   Optional correlation identifier for distributed tracing.
    /// </param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    Task EnqueueAsync(
        string eventType,
        string payloadJson,
        DateTimeOffset occurredAtUtc,
        string? correlationId,
        CancellationToken ct);
}
