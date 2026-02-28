using Lunara.Social.Application.Ports;

namespace Lunara.UnitTests.Social.Fakes;

/// <summary>Deterministic fake for <see cref="IClock"/> used in unit tests.</summary>
public sealed class FakeClock(DateTimeOffset utcNow) : IClock
{
    /// <inheritdoc/>
    public DateTimeOffset UtcNow { get; } = utcNow;
}
