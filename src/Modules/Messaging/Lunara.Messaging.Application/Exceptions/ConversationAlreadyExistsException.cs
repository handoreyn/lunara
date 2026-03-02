using Lunara.Messaging.Domain.ValueObjects;

namespace Lunara.Messaging.Application.Exceptions;

/// <summary>
/// Thrown by <see cref="UseCases.SendMessageService"/> (and its underlying infrastructure)
/// when a concurrent request has already created the conversation for the same
/// <see cref="MatchId"/>, violating the unique-per-match constraint.
/// </summary>
#pragma warning disable CA1032 // Domain-specific exception; callers should match on MatchId, not on a plain string message.
public sealed class ConversationAlreadyExistsException(MatchId matchId)
    : Exception($"A conversation for match '{matchId.Value}' already exists.")
{
    /// <summary>Gets the match identifier whose conversation already exists.</summary>
    public MatchId MatchId { get; } = matchId;
}
#pragma warning restore CA1032
