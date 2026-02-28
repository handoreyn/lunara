using Lunara.BuildingBlocks.Outbox;

namespace Lunara.UnitTests.Social.Fakes;

/// <summary>In-memory fake for <see cref="IOutboxWriter"/> used in unit tests.</summary>
public sealed class FakeOutboxWriter : IOutboxWriter
{
    /// <summary>Gets the number of times <see cref="EnqueueAsync"/> was called.</summary>
    public int EnqueueCallCount { get; private set; }

    /// <summary>Gets the event type passed to the most recent <see cref="EnqueueAsync"/> call.</summary>
    public string? LastEventType { get; private set; }

    /// <summary>Gets the payload JSON passed to the most recent <see cref="EnqueueAsync"/> call.</summary>
    public string? LastPayloadJson { get; private set; }

    /// <summary>Gets the correlation identifier passed to the most recent <see cref="EnqueueAsync"/> call.</summary>
    public string? LastCorrelationId { get; private set; }

    /// <summary>Gets the timestamp passed to the most recent <see cref="EnqueueAsync"/> call.</summary>
    public DateTimeOffset LastOccurredAtUtc { get; private set; }

    /// <inheritdoc/>
    public Task EnqueueAsync(
        string eventType,
        string payloadJson,
        DateTimeOffset occurredAtUtc,
        string? correlationId,
        CancellationToken ct)
    {
        EnqueueCallCount++;
        LastEventType = eventType;
        LastPayloadJson = payloadJson;
        LastOccurredAtUtc = occurredAtUtc;
        LastCorrelationId = correlationId;
        return Task.CompletedTask;
    }
}
