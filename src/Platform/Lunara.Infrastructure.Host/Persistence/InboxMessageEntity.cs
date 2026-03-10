namespace Lunara.Infrastructure.Host.Persistence;

/// <summary>
/// Persistent record of an inbound integration event used to enforce idempotent processing.
/// Implements the Inbox pattern: the first consumer to insert a row for a given
/// (EventId, Consumer) pair wins; subsequent duplicates are discarded.
/// </summary>
internal sealed class InboxMessageEntity
{
    /// <summary>Gets or sets the unique identifier of the originating event.</summary>
    public Guid EventId { get; set; }

    /// <summary>
    /// Gets or sets the logical name of the consuming module or handler
    /// (e.g. <c>"notifications"</c>). Forms the second part of the idempotency key.
    /// </summary>
    public string Consumer { get; set; } = string.Empty;

    /// <summary>Gets or sets the UTC timestamp of when the message was first received.</summary>
    public DateTimeOffset ReceivedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp of when the message was fully processed.
    /// <c>null</c> while the consumer is still handling the event.
    /// </summary>
    public DateTimeOffset? ProcessedAtUtc { get; set; }
}
