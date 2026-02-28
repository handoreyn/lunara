namespace Lunara.Infrastructure.Host.Outbox;

/// <summary>
/// Integer status codes stored in <c>OutboxMessageEntity.Status</c>.
/// Using constants rather than an enum avoids an extra cast in EF Core LINQ queries.
/// </summary>
internal static class OutboxMessageStatus
{
    /// <summary>The message is waiting to be published.</summary>
    public const int Pending = 0;

    /// <summary>The message was successfully delivered to the broker.</summary>
    public const int Sent = 2;

    /// <summary>
    /// Delivery was attempted the maximum number of times and all attempts failed.
    /// Manual intervention is required.
    /// </summary>
    public const int Failed = 3;
}
