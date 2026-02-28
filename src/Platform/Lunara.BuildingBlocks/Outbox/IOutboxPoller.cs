namespace Lunara.BuildingBlocks.Outbox;

/// <summary>
/// Polls the outbox table for pending messages and publishes them to the message broker.
/// Implementations are short-lived (scoped) and must be resolved inside a service scope.
/// </summary>
public interface IOutboxPoller
{
    /// <summary>
    /// Processes one batch of pending outbox messages and returns the number published.
    /// </summary>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>The count of messages processed in this batch.</returns>
    Task<int> PollOnceAsync(CancellationToken ct);
}
