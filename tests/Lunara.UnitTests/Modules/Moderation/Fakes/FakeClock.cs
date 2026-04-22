using Lunara.BuildingBlocks.Clocks;

namespace Lunara.UnitTests.Modules.Moderation.Fakes;

/// <summary>Deterministic fake for <see cref="IClock"/> used in Moderation unit tests.</summary>
public sealed class FakeClock(DateTimeOffset utcNow) : IClock
{
    /// <inheritdoc/>
    public DateTimeOffset UtcNow { get; } = utcNow;
}
