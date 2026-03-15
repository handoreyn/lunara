using Lunara.BuildingBlocks.Moderation;

namespace Lunara.UnitTests.Modules.Messaging.Fakes;

/// <summary>
/// Configurable fake for <see cref="IBlockChecker"/> used in Messaging unit tests.
/// </summary>
public sealed class FakeBlockChecker : IBlockChecker
{
    private readonly HashSet<(Guid, Guid)> _blocked = [];

    /// <summary>Seeds a directional block so blocked scenarios can be exercised.</summary>
    public void SeedBlock(Guid blockerId, Guid blockedId)
    {
        _blocked.Add((blockerId, blockedId));
    }

    /// <inheritdoc/>
    public Task<bool> IsBlockedInEitherDirectionAsync(Guid userId1, Guid userId2, CancellationToken ct)
    {
        bool blocked = _blocked.Contains((userId1, userId2))
                    || _blocked.Contains((userId2, userId1));
        return Task.FromResult(blocked);
    }
}
