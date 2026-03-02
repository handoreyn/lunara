using Lunara.Messaging.Application.Ports;

namespace Lunara.UnitTests.Modules.Messaging.Fakes;

/// <summary>Deterministic fake for <see cref="IClock"/> used in Messaging unit tests.</summary>
public sealed class FakeClock(DateTimeOffset utcNow) : IClock
{
    /// <inheritdoc/>
    public DateTimeOffset UtcNow { get; } = utcNow;
}
