using Lunara.BuildingBlocks.Clocks;

namespace Lunara.UnitTests.Moderation.Fakes;

/// <summary>Deterministic fake for <see cref="IClock"/> used in Moderation unit tests.</summary>
public sealed class FakeModerationClock(DateTimeOffset utcNow) : IClock
{
    /// <inheritdoc/>
    public DateTimeOffset UtcNow { get; } = utcNow;
}
