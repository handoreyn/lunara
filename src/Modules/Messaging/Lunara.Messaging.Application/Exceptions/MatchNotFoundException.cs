using Lunara.Messaging.Domain.ValueObjects;

namespace Lunara.Messaging.Application.Exceptions;

/// <summary>
/// Thrown by <see cref="UseCases.SendMessageService"/> when the requested
/// <see cref="MatchId"/> does not exist or is no longer valid.
/// </summary>
#pragma warning disable CA1032 // Domain-specific exception; callers should match on MatchId, not on a plain string message.
public sealed class MatchNotFoundException(MatchId matchId)
    : Exception($"Match '{matchId.Value}' was not found.")
{
    /// <summary>Gets the match identifier that could not be found.</summary>
    public MatchId MatchId { get; } = matchId;
}
#pragma warning restore CA1032
