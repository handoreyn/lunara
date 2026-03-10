namespace Lunara.BuildingBlocks.Inbox;

/// <summary>
/// Represents a received integration event recorded in the inbox for idempotent processing.
/// This is a plain data model; persistence mapping is owned by each module's Infrastructure layer.
/// </summary>
/// <param name="Id">The unique identifier of the originating event.</param>
/// <param name="Consumer">
///   Logical name of the consuming module or handler (e.g. <c>"notifications"</c>).
///   Combined with <paramref name="Id"/> it forms the idempotency key.
/// </param>
/// <param name="ReceivedAtUtc">UTC timestamp of when the message was first received.</param>
public sealed record InboxMessage(
    Guid Id,
    string Consumer,
    DateTimeOffset ReceivedAtUtc);
