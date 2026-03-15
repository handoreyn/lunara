using Lunara.Social.Domain.Entities;
using Lunara.Social.Domain.ValueObjects;

namespace Lunara.Social.Application.Ports;

/// <summary>Persistence port for storing and querying matches.</summary>
public interface IMatchRepository
{
    /// <summary>Persists a newly created <paramref name="match"/>.</summary>
    Task AddAsync(Match match, CancellationToken ct);

    /// <summary>
    /// Returns <c>true</c> when a match with the given <paramref name="matchId"/> exists.
    /// </summary>
    /// <param name="matchId">The match to check.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    Task<bool> ExistsAsync(MatchId matchId, CancellationToken ct);

    /// <summary>
    /// Returns the <see cref="Match"/> with the given <paramref name="matchId"/>,
    /// or <c>null</c> if no such match exists.
    /// </summary>
    /// <param name="matchId">The match identifier to look up.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    Task<Match?> FindByIdAsync(MatchId matchId, CancellationToken ct);
}
