using Lunara.Social.Application.Ports;
using Lunara.Social.Domain.Entities;

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
}
