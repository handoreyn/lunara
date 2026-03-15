using Lunara.Moderation.Application.Ports;
using Lunara.Moderation.Domain.Entities;
using Lunara.Moderation.Domain.ValueObjects;

namespace Lunara.UnitTests.Moderation.Fakes;

/// <summary>In-memory fake for <see cref="IBlockRepository"/> used in unit tests.</summary>
public sealed class FakeBlockRepository : IBlockRepository
{
    private readonly HashSet<(UserId Blocker, UserId Blocked)> _blocks = [];

    /// <summary>Gets the number of times <see cref="AddAsync"/> was called.</summary>
    public int AddCallCount { get; private set; }

    /// <summary>Seeds an existing block so duplicate-block and direction logic can be exercised.</summary>
    public void SeedBlock(UserId blocker, UserId blocked)
    {
        _blocks.Add((blocker, blocked));
    }

    /// <inheritdoc/>
    public Task<bool> ExistsAsync(UserId blockerUserId, UserId blockedUserId, CancellationToken ct)
    {
        return Task.FromResult(_blocks.Contains((blockerUserId, blockedUserId)));
    }

    /// <inheritdoc/>
    public Task AddAsync(Block block, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(block);
        _blocks.Add((block.BlockerUserId, block.BlockedUserId));
        AddCallCount++;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<bool> IsBlockedInEitherDirectionAsync(UserId userId1, UserId userId2, CancellationToken ct)
    {
        bool blocked = _blocks.Contains((userId1, userId2))
                    || _blocks.Contains((userId2, userId1));
        return Task.FromResult(blocked);
    }
}
