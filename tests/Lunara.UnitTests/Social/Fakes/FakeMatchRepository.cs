using Lunara.Social.Application.Ports;
using Lunara.Social.Domain.Entities;
using Lunara.Social.Domain.ValueObjects;

namespace Lunara.UnitTests.Social.Fakes;

/// <summary>In-memory fake for <see cref="IMatchRepository"/> used in unit tests.</summary>
public sealed class FakeMatchRepository : IMatchRepository
{
    private readonly List<Match> _addedMatches = [];

    /// <summary>Gets all matches that were passed to <see cref="AddAsync"/>.</summary>
    public IReadOnlyList<Match> AddedMatches => _addedMatches;

    /// <inheritdoc/>
    public Task AddAsync(Match match, CancellationToken ct)
    {
        _addedMatches.Add(match);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<bool> ExistsAsync(MatchId matchId, CancellationToken ct)
    {
        bool exists = _addedMatches.Any(m => m.Id == matchId);
        return Task.FromResult(exists);
    }

    /// <inheritdoc/>
    public Task<Match?> FindByIdAsync(MatchId matchId, CancellationToken ct)
    {
        Match? match = _addedMatches.FirstOrDefault(m => m.Id == matchId);
        return Task.FromResult(match);
    }
}
