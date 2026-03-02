using Lunara.Social.Application.Ports;
using Lunara.Social.Domain.Entities;
using Lunara.Social.Domain.ValueObjects;

namespace Lunara.Infrastructure.Host.Social;

/// <summary>
/// Thread-safe in-memory implementation of <see cref="IMatchRepository"/> for local development.
/// Data is not persisted across application restarts.
/// </summary>
internal sealed class InMemoryMatchRepository : IMatchRepository
{
    private readonly List<Match> _matches = [];
    private readonly Lock _lock = new();

    /// <inheritdoc/>
    public Task AddAsync(Match match, CancellationToken ct)
    {
        lock (_lock)
        {
            _matches.Add(match);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<bool> ExistsAsync(MatchId matchId, CancellationToken ct)
    {
        bool exists;
        lock (_lock)
        {
            exists = _matches.Any(m => m.Id == matchId);
        }

        return Task.FromResult(exists);
    }
}
