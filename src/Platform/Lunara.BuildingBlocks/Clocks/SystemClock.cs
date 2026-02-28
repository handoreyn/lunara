namespace Lunara.BuildingBlocks.Clocks;

/// <summary>
/// Production implementation of <see cref="IClock"/> that delegates to <see cref="DateTimeOffset.UtcNow"/>.
/// </summary>
public sealed class SystemClock : IClock
{
    /// <inheritdoc/>
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
