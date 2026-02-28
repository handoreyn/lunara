namespace Lunara.Infrastructure.Host.Persistence;

/// <summary>
/// Persistent record of an integration event yet to be published to the message broker.
/// Implements the Transactional Outbox pattern.
/// </summary>
internal sealed class OutboxMessageEntity
{
    /// <summary>Gets or sets the unique identifier for this outbox entry.</summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the short, dot-separated event type name
    /// (e.g. <c>"social.match.created"</c>).
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>Gets or sets the serialised event payload as a JSON string.</summary>
    public string PayloadJson { get; set; } = string.Empty;

    /// <summary>Gets or sets the UTC timestamp of when the domain event occurred.</summary>
    public DateTimeOffset OccurredAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the processing status.
    /// <c>0</c> = Pending, <c>1</c> = Processing, <c>2</c> = Sent, <c>3</c> = Failed.
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp until which this entry is locked by a processor.
    /// <c>null</c> means the entry is not currently locked.
    /// </summary>
    public DateTimeOffset? LockedUntilUtc { get; set; }

    /// <summary>Gets or sets the number of delivery attempts made so far.</summary>
    public int Attempts { get; set; }

    /// <summary>
    /// Gets or sets the most recent error message encountered during delivery.
    /// <c>null</c> when no errors have occurred.
    /// </summary>
    public string? LastError { get; set; }
}
