using Lunara.BuildingBlocks.Clocks;

namespace Lunara.UnitTests.Moderation.Fakes;

/// <summary>Deterministic fake clock for moderation unit tests.</summary>
public sealed class FakeModerationClock(DateTimeOffset utcNow) : IClock
{
    /// <inheritdoc/>
    public DateTimeOffset UtcNow => utcNow;
}
