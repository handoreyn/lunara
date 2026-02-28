using Lunara.Social.Application.DTOs;
using Lunara.Social.Application.UseCases;
using Lunara.Social.Domain;
using Lunara.Social.Domain.ValueObjects;
using Lunara.UnitTests.Social.Fakes;

namespace Lunara.UnitTests.Social;

/// <summary>Unit tests for <see cref="RecordSwipeService"/>.</summary>
public sealed class RecordSwipeServiceTests
{
    private static readonly DateTimeOffset FixedNow =
        new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static readonly UserId ActorId = new(new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
    private static readonly UserId TargetId = new(new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));

    private static (RecordSwipeService Svc, FakeLikeRepository Likes, FakeMatchRepository Matches, FakeUnitOfWork Uow)
        Build(FakeLikeRepository? likes = null)
    {
        FakeLikeRepository likeRepo = likes ?? new FakeLikeRepository();
        FakeMatchRepository matchRepo = new();
        FakeUnitOfWork uow = new();
        FakeClock clock = new(FixedNow);
        RecordSwipeService svc = new(likeRepo, matchRepo, uow, clock);
        return (svc, likeRepo, matchRepo, uow);
    }

    /// <summary>A non-reciprocal like records a like and saves once, no match is created.</summary>
    [Fact]
    public async Task Like_NonReciprocal_RecordsLike_NoMatch_SavesOnce()
    {
        (RecordSwipeService svc, FakeLikeRepository likes, FakeMatchRepository matches, FakeUnitOfWork uow) = Build();

        RecordSwipeResult result = await svc.RecordAsync(
            new RecordSwipeRequest(ActorId, TargetId, SwipeActionType.Like),
            CancellationToken.None);

        Assert.True(result.LikeRecorded);
        Assert.False(result.MatchCreated);
        Assert.Null(result.MatchId);
        Assert.Equal(1, likes.AddLikeCallCount);
        Assert.Empty(matches.AddedMatches);
        Assert.Equal(1, uow.SaveCallCount);
    }

    /// <summary>A reciprocal like records a like, creates a match, and saves once.</summary>
    [Fact]
    public async Task Like_Reciprocal_RecordsLike_CreatesMatch_SavesOnce()
    {
        FakeLikeRepository likes = new();
        likes.SeedLike(TargetId, ActorId);
        (RecordSwipeService svc, _, FakeMatchRepository matches, FakeUnitOfWork uow) = Build(likes);

        RecordSwipeResult result = await svc.RecordAsync(
            new RecordSwipeRequest(ActorId, TargetId, SwipeActionType.Like),
            CancellationToken.None);

        Assert.True(result.LikeRecorded);
        Assert.True(result.MatchCreated);
        Assert.NotNull(result.MatchId);
        Assert.Equal(1, likes.AddLikeCallCount);
        Assert.Single(matches.AddedMatches);
        Assert.Equal(1, uow.SaveCallCount);
    }

    /// <summary>A duplicate like is silently ignored — no add, no save, no match.</summary>
    [Fact]
    public async Task Duplicate_Like_NoAddLike_NoSave_NoMatch()
    {
        FakeLikeRepository likes = new();
        likes.SeedLike(ActorId, TargetId);
        (RecordSwipeService svc, _, FakeMatchRepository matches, FakeUnitOfWork uow) = Build(likes);

        RecordSwipeResult result = await svc.RecordAsync(
            new RecordSwipeRequest(ActorId, TargetId, SwipeActionType.Like),
            CancellationToken.None);

        Assert.False(result.LikeRecorded);
        Assert.False(result.MatchCreated);
        Assert.Null(result.MatchId);
        Assert.Equal(0, likes.AddLikeCallCount);
        Assert.Empty(matches.AddedMatches);
        Assert.Equal(0, uow.SaveCallCount);
    }

    /// <summary>A Pass swipe makes no repository calls and no save.</summary>
    [Fact]
    public async Task Pass_NoCalls_NoSave()
    {
        (RecordSwipeService svc, FakeLikeRepository likes, FakeMatchRepository matches, FakeUnitOfWork uow) = Build();

        RecordSwipeResult result = await svc.RecordAsync(
            new RecordSwipeRequest(ActorId, TargetId, SwipeActionType.Pass),
            CancellationToken.None);

        Assert.False(result.LikeRecorded);
        Assert.False(result.MatchCreated);
        Assert.Null(result.MatchId);
        Assert.Equal(0, likes.AddLikeCallCount);
        Assert.Empty(matches.AddedMatches);
        Assert.Equal(0, uow.SaveCallCount);
    }

    /// <summary>Swiping on yourself throws <see cref="ArgumentException"/>.</summary>
    [Fact]
    public async Task SameUser_Throws_ArgumentException()
    {
        (RecordSwipeService svc, _, _, _) = Build();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            svc.RecordAsync(
                new RecordSwipeRequest(ActorId, ActorId, SwipeActionType.Like),
                CancellationToken.None));
    }
}
