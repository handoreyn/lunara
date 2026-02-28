using Lunara.Social.Domain;
using Lunara.Social.Domain.DomainEvents;
using Lunara.Social.Domain.Entities;
using Lunara.Social.Domain.ValueObjects;

namespace Lunara.UnitTests.Social;

public sealed class SocialDomainTests
{
    // ── UserId ─────────────────────────────────────────────────────────────

    [Fact]
    public void UserId_With_Empty_Guid_Throws_ArgumentException()
    {
        ArgumentException ex = Assert.Throws<ArgumentException>(
            () => _ = new UserId(Guid.Empty));

        Assert.Contains("UserId must not be empty", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void UserId_With_Valid_Guid_Succeeds()
    {
        Guid guid = Guid.NewGuid();
        UserId id = new(guid);
        Assert.Equal(guid, id.Value);
    }

    // ── MatchId ────────────────────────────────────────────────────────────

    [Fact]
    public void MatchId_With_Empty_Guid_Throws_ArgumentException()
    {
        ArgumentException ex = Assert.Throws<ArgumentException>(
            () => _ = new MatchId(Guid.Empty));

        Assert.Contains("MatchId must not be empty", ex.Message, StringComparison.Ordinal);
    }

    // ── Match canonical ordering ───────────────────────────────────────────

    [Fact]
    public void Match_Canonical_Ordering_Stores_Smaller_Guid_As_User1Id()
    {
        // Arrange: pick two distinct Guids and identify which is smaller.
        Guid rawA = new("00000000-0000-0000-0000-000000000001");
        Guid rawB = new("ffffffff-ffff-ffff-ffff-ffffffffffff");

        UserId actorId = new(rawB);  // larger
        UserId targetId = new(rawA); // smaller

        // Act – actor is larger, so targetId should become User1Id.
        (Match? match, MatchCreated? _) = Match.CreateMatchIfReciprocalLike(
            actorId, targetId, DateTimeOffset.UtcNow, reciprocalLikeExists: true);

        // Assert
        Assert.NotNull(match);
        Assert.Equal(targetId, match.User1Id);
        Assert.Equal(actorId, match.User2Id);
    }

    [Fact]
    public void Match_Canonical_Ordering_When_Actor_Is_Smaller_Guid()
    {
        Guid rawA = new("00000000-0000-0000-0000-000000000001");
        Guid rawB = new("ffffffff-ffff-ffff-ffff-ffffffffffff");

        UserId actorId = new(rawA);  // smaller
        UserId targetId = new(rawB); // larger

        (Match? match, MatchCreated? _) = Match.CreateMatchIfReciprocalLike(
            actorId, targetId, DateTimeOffset.UtcNow, reciprocalLikeExists: true);

        Assert.NotNull(match);
        Assert.Equal(actorId, match.User1Id);
        Assert.Equal(targetId, match.User2Id);
    }

    // ── CreateMatchIfReciprocalLike – non-reciprocal ───────────────────────

    [Fact]
    public void CreateMatchIfReciprocalLike_NonReciprocal_Returns_Nulls()
    {
        UserId actorId = new(Guid.NewGuid());
        UserId targetId = new(Guid.NewGuid());

        (Match? match, MatchCreated? evt) = Match.CreateMatchIfReciprocalLike(
            actorId, targetId, DateTimeOffset.UtcNow, reciprocalLikeExists: false);

        Assert.Null(match);
        Assert.Null(evt);
    }

    [Fact]
    public void CreateMatchIfReciprocalLike_Reciprocal_Returns_MatchAndEvent()
    {
        UserId actorId = new(Guid.NewGuid());
        UserId targetId = new(Guid.NewGuid());
        DateTimeOffset now = DateTimeOffset.UtcNow;

        (Match? match, MatchCreated? evt) = Match.CreateMatchIfReciprocalLike(
            actorId, targetId, now, reciprocalLikeExists: true);

        Assert.NotNull(match);
        Assert.NotNull(evt);
        Assert.Equal(match.Id, evt.MatchId);
        Assert.Equal(match.CreatedAtUtc, now);
    }

    // ── CreateMatchIfReciprocalLike – same user guard ─────────────────────

    [Fact]
    public void CreateMatchIfReciprocalLike_SameUser_Throws_ArgumentException()
    {
        UserId userId = new(Guid.NewGuid());

        ArgumentException ex = Assert.Throws<ArgumentException>(
            () => Match.CreateMatchIfReciprocalLike(
                userId, userId, DateTimeOffset.UtcNow, reciprocalLikeExists: true));

        Assert.Contains("different users", ex.Message, StringComparison.Ordinal);
    }

    // ── SwipeActionType enum ───────────────────────────────────────────────

    [Fact]
    public void SwipeActionType_Has_Like_And_Pass_Values()
    {
        Assert.Equal(0, (int)SwipeActionType.Like);
        Assert.Equal(1, (int)SwipeActionType.Pass);
    }
}
