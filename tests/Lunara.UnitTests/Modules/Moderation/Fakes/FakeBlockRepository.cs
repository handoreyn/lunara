using Lunara.Moderation.Application.Ports;
using Lunara.Moderation.Domain.Entities;
using Lunara.Moderation.Domain.ValueObjects;

namespace Lunara.UnitTests.Modules.Moderation.Fakes;

/// <summary>
/// In-memory fake for <see cref="IBlockRepository"/> used in Moderation unit tests.
/// </summary>
public sealed class FakeBlockRepository : IBlockRepository
{
    private readonly List<Block> _blocks = [];

    /// <summary>Gets all blocks that were added via <see cref="AddAsync"/>.</summary>
    public IReadOnlyList<Block> AddedBlocks => _blocks.AsReadOnly();

    /// <summary>Gets the number of times <see cref="AddAsync"/> was called.</summary>
    public int AddCallCount { get; private set; }

    /// <inheritdoc/>
    public Task<bool> ExistsAsync(UserId blockerUserId, UserId blockedUserId, CancellationToken ct)
    {
        bool exists = _blocks.Exists(b =>
            b.BlockerUserId == blockerUserId && b.BlockedUserId == blockedUserId);
        return Task.FromResult(exists);
    }

    /// <inheritdoc/>
    public Task<bool> ExistsInEitherDirectionAsync(UserId userA, UserId userB, CancellationToken ct)
    {
        bool exists = _blocks.Exists(b =>
            (b.BlockerUserId == userA && b.BlockedUserId == userB)
         || (b.BlockerUserId == userB && b.BlockedUserId == userA));
        return Task.FromResult(exists);
    }

    /// <inheritdoc/>
    public Task AddAsync(Block block, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(block);
        AddCallCount++;
        _blocks.Add(block);
        return Task.CompletedTask;
    }
}
