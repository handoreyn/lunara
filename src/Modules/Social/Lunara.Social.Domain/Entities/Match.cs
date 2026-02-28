using Lunara.Social.Domain.DomainEvents;
using Lunara.Social.Domain.ValueObjects;

namespace Lunara.Social.Domain.Entities;

/// <summary>
/// Aggregate root representing a mutual match between two users.
/// <para>
/// The pair is stored in canonical order: <see cref="User1Id"/> always holds the
/// lexicographically smaller <see cref="Guid"/>, <see cref="User2Id"/> the larger.
/// This guarantees that a given pair is represented by exactly one <see cref="Match"/> record.
/// </para>
/// </summary>
public sealed class Match
{
    /// <summary>Gets the unique identifier of this match.</summary>
    public MatchId Id { get; }

    /// <summary>Gets the user with the lexicographically smaller user identifier.</summary>
    public UserId User1Id { get; }

    /// <summary>Gets the user with the lexicographically larger user identifier.</summary>
    public UserId User2Id { get; }

    /// <summary>Gets the UTC timestamp at which the match was created.</summary>
    public DateTimeOffset CreatedAtUtc { get; }

    private Match(MatchId id, UserId user1Id, UserId user2Id, DateTimeOffset createdAtUtc)
    {
        Id = id;
        User1Id = user1Id;
        User2Id = user2Id;
        CreatedAtUtc = createdAtUtc;
    }

    /// <summary>
    /// Creates a <see cref="Match"/> and the associated <see cref="MatchCreated"/> domain event
    /// when a reciprocal like exists; otherwise returns <c>(null, null)</c>.
    /// </summary>
    /// <param name="actorId">The user who just swiped like.</param>
    /// <param name="targetId">The user who was swiped on.</param>
    /// <param name="occurredAtUtc">The UTC timestamp of the swipe action.</param>
    /// <param name="reciprocalLikeExists">
    ///   <c>true</c> if <paramref name="targetId"/> has already liked <paramref name="actorId"/>.
    /// </param>
    /// <returns>
    ///   A <see cref="Match"/> and its <see cref="MatchCreated"/> event when
    ///   <paramref name="reciprocalLikeExists"/> is <c>true</c>; otherwise <c>(null, null)</c>.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///   Thrown when <paramref name="actorId"/> equals <paramref name="targetId"/>.
    /// </exception>
    public static (Match? Match, MatchCreated? Event) CreateMatchIfReciprocalLike(
        UserId actorId,
        UserId targetId,
        DateTimeOffset occurredAtUtc,
        bool reciprocalLikeExists)
    {
        if (actorId == targetId)
        {
            throw new ArgumentException(
                "Actor and target must be different users.",
                nameof(targetId));
        }

        if (!reciprocalLikeExists)
        {
            return (null, null);
        }

        // Canonical ordering: smaller Guid → User1Id, larger → User2Id.
        bool actorFirst = actorId.Value.CompareTo(targetId.Value) < 0;
        UserId user1Id = actorFirst ? actorId : targetId;
        UserId user2Id = actorFirst ? targetId : actorId;

        MatchId matchId = new(Guid.NewGuid());
        Match match = new(matchId, user1Id, user2Id, occurredAtUtc);
        MatchCreated evt = new(matchId, user1Id, user2Id, occurredAtUtc);

        return (match, evt);
    }
}
