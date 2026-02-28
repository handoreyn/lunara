using System.Collections.Concurrent;
using Lunara.Social.Application.Ports;
using Lunara.Social.Domain.ValueObjects;

namespace Lunara.Infrastructure.Host.Social;

/// <summary>
/// Thread-safe in-memory implementation of <see cref="ILikeRepository"/> for local development.
/// Data is not persisted across application restarts.
/// </summary>
internal sealed class InMemoryLikeRepository : ILikeRepository
{
    // Key: (From, Recipient) tuple stored as (Guid, Guid) to keep the dict key simple.
    private readonly ConcurrentDictionary<(Guid From, Guid Recipient), DateTimeOffset> _likes = new();

    /// <inheritdoc/>
    public Task<bool> ExistsLikeAsync(UserId from, UserId recipient, CancellationToken ct)
    {
        bool exists = _likes.ContainsKey((from.Value, recipient.Value));
        return Task.FromResult(exists);
    }

    /// <inheritdoc/>
    public Task AddLikeAsync(UserId from, UserId recipient, DateTimeOffset atUtc, CancellationToken ct)
    {
        _likes.TryAdd((from.Value, recipient.Value), atUtc);
        return Task.CompletedTask;
    }
}
