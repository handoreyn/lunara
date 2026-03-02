using Lunara.Messaging.Domain.ValueObjects;

namespace Lunara.Messaging.Application.Ports;

/// <summary>
/// Read-only port for checking the existence of a social match.
/// Implemented by the Social module's infrastructure (or an anti-corruption adapter).
/// </summary>
public interface IMatchReadService
{
    /// <summary>
    /// Returns <c>true</c> when a match with the given <paramref name="matchId"/> exists.
    /// </summary>
    /// <param name="matchId">The match to verify.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    Task<bool> MatchExistsAsync(MatchId matchId, CancellationToken ct);
}
