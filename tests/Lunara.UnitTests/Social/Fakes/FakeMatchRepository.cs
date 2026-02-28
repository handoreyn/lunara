using Lunara.Social.Application.Ports;
using Lunara.Social.Domain.Entities;

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
}
