using Lunara.BuildingBlocks.Moderation;

namespace Lunara.UnitTests.Social.Fakes;

/// <summary>
/// Configurable fake for <see cref="IBlockChecker"/> used in Social unit tests.
/// </summary>
public sealed class FakeBlockChecker : IBlockChecker
{
    private readonly HashSet<(Guid, Guid)> _blocked = [];

    /// <summary>Seeds a bidirectional block so blocked-user scenarios can be exercised.</summary>
    public void SeedBlock(Guid userId1, Guid userId2)
    {
        _blocked.Add((userId1, userId2));
    }

    /// <inheritdoc/>
    public Task<bool> IsBlockedInEitherDirectionAsync(Guid userId1, Guid userId2, CancellationToken ct)
    {
        bool blocked = _blocked.Contains((userId1, userId2))
                    || _blocked.Contains((userId2, userId1));
        return Task.FromResult(blocked);
    }
}
