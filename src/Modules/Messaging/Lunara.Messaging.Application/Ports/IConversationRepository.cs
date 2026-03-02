using Lunara.Messaging.Domain.Entities;
using Lunara.Messaging.Domain.ValueObjects;

namespace Lunara.Messaging.Application.Ports;

/// <summary>Persistence port for <see cref="Conversation"/> aggregates.</summary>
public interface IConversationRepository
{
    /// <summary>
    /// Returns the <see cref="Conversation"/> whose originating match equals
    /// <paramref name="matchId"/>, or <c>null</c> if no such conversation exists yet.
    /// </summary>
    /// <param name="matchId">The match identifier to look up.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    Task<Conversation?> GetByMatchIdAsync(MatchId matchId, CancellationToken ct);

    /// <summary>Persists a newly created <see cref="Conversation"/>.</summary>
    /// <param name="conversation">The conversation to add.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    Task AddAsync(Conversation conversation, CancellationToken ct);
}
