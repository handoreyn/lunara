using Lunara.Messaging.Application.Ports;

namespace Lunara.UnitTests.Messaging.Fakes;

/// <summary>Deterministic fake clock for Messaging unit tests.</summary>
public sealed class FakeMessagingClock(DateTimeOffset utcNow) : IClock
{
    /// <inheritdoc/>
    public DateTimeOffset UtcNow => utcNow;
}
