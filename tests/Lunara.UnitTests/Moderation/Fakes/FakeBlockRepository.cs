using Lunara.Moderation.Application.Ports;
using Lunara.Moderation.Domain.Entities;
using Lunara.Moderation.Domain.ValueObjects;

namespace Lunara.UnitTests.Moderation.Fakes;

/// <summary>In-memory fake for <see cref="IBlockRepository"/> used in unit tests.</summary>
public sealed class FakeBlockRepository : IBlockRepository
{
    private readonly HashSet<(Guid Blocker, Guid Blocked)> _blocks = [];

    /// <summary>Gets all blocks that have been added.</summary>
    public IReadOnlyCollection<(Guid Blocker, Guid Blocked)> Blocks => _blocks;

    /// <summary>Seeds an existing block so duplicate-block logic can be exercised.</summary>
    public void SeedBlock(UserId blocker, UserId blocked)
    {
        _blocks.Add((blocker.Value, blocked.Value));
    }

    /// <inheritdoc/>
    public Task<bool> ExistsAsync(UserId blockerUserId, UserId blockedUserId, CancellationToken ct)
    {
        return Task.FromResult(_blocks.Contains((blockerUserId.Value, blockedUserId.Value)));
    }

    /// <inheritdoc/>
    public Task<bool> IsBlockedInEitherDirectionAsync(UserId userId1, UserId userId2, CancellationToken ct)
    {
        bool blocked = _blocks.Contains((userId1.Value, userId2.Value))
                    || _blocks.Contains((userId2.Value, userId1.Value));
        return Task.FromResult(blocked);
    }

    /// <inheritdoc/>
    public Task AddAsync(Block block, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(block);
        _blocks.Add((block.BlockerUserId.Value, block.BlockedUserId.Value));
        return Task.CompletedTask;
    }
}
