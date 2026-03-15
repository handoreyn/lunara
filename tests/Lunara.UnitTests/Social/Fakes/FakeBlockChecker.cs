using Lunara.BuildingBlocks.Moderation;

namespace Lunara.UnitTests.Social.Fakes;

/// <summary>
/// In-memory fake for <see cref="IBlockChecker"/> used in Social unit tests.
/// All checks return <c>false</c> (not blocked) by default.
/// Call <see cref="SetBlocked"/> to simulate a block.
/// </summary>
public sealed class FakeBlockChecker : IBlockChecker
{
    private readonly HashSet<(Guid, Guid)> _blocked = [];

    /// <summary>Registers a block between <paramref name="userA"/> and <paramref name="userB"/> (bidirectional).</summary>
    public void SetBlocked(Guid userA, Guid userB)
    {
        _blocked.Add((userA, userB));
        _blocked.Add((userB, userA));
    }

    /// <inheritdoc/>
    public Task<bool> IsBlockedAsync(Guid userA, Guid userB, CancellationToken ct)
    {
        return Task.FromResult(_blocked.Contains((userA, userB)) || _blocked.Contains((userB, userA)));
    }
}
