using Lunara.BuildingBlocks.Moderation;

namespace Lunara.UnitTests.Messaging.Fakes;

/// <summary>Configurable fake for <see cref="IBlockChecker"/> used in Messaging unit tests.</summary>
public sealed class FakeMessagingBlockChecker : IBlockChecker
{
    private bool _blocked;

    /// <summary>
    /// Configures the fake to return <c>true</c> from
    /// <see cref="IsBlockedInEitherDirectionAsync"/> for all user pairs.
    /// </summary>
    public void SetBlocked(bool blocked)
    {
        _blocked = blocked;
    }

    /// <inheritdoc/>
    public Task<bool> IsBlockedInEitherDirectionAsync(Guid userId1, Guid userId2, CancellationToken ct)
    {
        return Task.FromResult(_blocked);
    }
}
