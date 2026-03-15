using System.Collections.Concurrent;
using Lunara.Moderation.Application.Ports;
using Lunara.Moderation.Domain.Entities;
using Lunara.Moderation.Domain.ValueObjects;

namespace Lunara.Infrastructure.Host.Moderation;

/// <summary>
/// Thread-safe in-memory implementation of <see cref="IBlockRepository"/> for local development.
/// Data is not persisted across application restarts.
/// </summary>
internal sealed class InMemoryBlockRepository : IBlockRepository
{
    private readonly ConcurrentDictionary<(Guid Blocker, Guid Blocked), DateTimeOffset> _blocks = new();

    /// <inheritdoc/>
    public Task<bool> ExistsAsync(UserId blockerUserId, UserId blockedUserId, CancellationToken ct)
    {
        bool exists = _blocks.ContainsKey((blockerUserId.Value, blockedUserId.Value));
        return Task.FromResult(exists);
    }

    /// <inheritdoc/>
    public Task<bool> IsBlockedInEitherDirectionAsync(UserId userId1, UserId userId2, CancellationToken ct)
    {
        bool blocked = _blocks.ContainsKey((userId1.Value, userId2.Value))
                    || _blocks.ContainsKey((userId2.Value, userId1.Value));
        return Task.FromResult(blocked);
    }

    /// <inheritdoc/>
    public Task AddAsync(Block block, CancellationToken ct)
    {
        _blocks.TryAdd((block.BlockerUserId.Value, block.BlockedUserId.Value), block.CreatedAtUtc);
        return Task.CompletedTask;
    }
}
