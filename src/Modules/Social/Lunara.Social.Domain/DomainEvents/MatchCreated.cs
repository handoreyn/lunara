using Lunara.Social.Domain.ValueObjects;

namespace Lunara.Social.Domain.DomainEvents;

/// <summary>
/// Raised when two users have mutually liked each other and a <see cref="Entities.Match"/> has been created.
/// </summary>
/// <param name="MatchId">The identifier of the newly created match.</param>
/// <param name="User1Id">The user with the lexicographically smaller identifier.</param>
/// <param name="User2Id">The user with the lexicographically larger identifier.</param>
/// <param name="OccurredAtUtc">The UTC timestamp at which the match was formed.</param>
public sealed record MatchCreated(
    MatchId MatchId,
    UserId User1Id,
    UserId User2Id,
    DateTimeOffset OccurredAtUtc);
