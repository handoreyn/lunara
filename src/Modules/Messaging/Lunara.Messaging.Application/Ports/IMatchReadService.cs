using Lunara.Messaging.Domain.ValueObjects;

namespace Lunara.Messaging.Application.Ports;

/// <summary>
/// Read-only port for querying a social match's participants.
/// Implemented by the Social module's infrastructure (or an anti-corruption adapter).
/// </summary>
public interface IMatchReadService
{
    /// <summary>
    /// Returns the canonical participant pair for the match with the given
    /// <paramref name="matchId"/>, or <c>null</c> if no such match exists.
    /// </summary>
    /// <param name="matchId">The match to look up.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    Task<MatchParticipants?> GetParticipantsAsync(MatchId matchId, CancellationToken ct);
}
