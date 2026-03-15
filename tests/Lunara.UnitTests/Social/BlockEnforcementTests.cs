using Lunara.Social.Application.DTOs;
using Lunara.Social.Application.UseCases;
using Lunara.Social.Domain;
using Lunara.Social.Domain.ValueObjects;
using Lunara.UnitTests.Social.Fakes;

namespace Lunara.UnitTests.Social;

/// <summary>Unit tests for block-enforcement rules in <see cref="RecordSwipeService"/>.</summary>
public sealed class BlockEnforcementTests
{
    private static readonly DateTimeOffset FixedNow = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly UserId ActorId = new(new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
    private static readonly UserId TargetId = new(new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));

    private static RecordSwipeService BuildBlocked()
    {
        FakeBlockChecker blockChecker = new();
        blockChecker.SetBlocked(true);
        return new RecordSwipeService(
            new FakeLikeRepository(),
            new FakeMatchRepository(),
            new FakeUnitOfWork(),
            new FakeClock(FixedNow),
            new FakeOutboxWriter(),
            blockChecker);
    }

    /// <summary>A like is rejected and marked blocked when a block exists in either direction.</summary>
    [Fact]
    public async Task Like_WhenBlocked_ReturnsBlockedResult_NothingPersisted()
    {
        RecordSwipeService svc = BuildBlocked();

        RecordSwipeResult result = await svc.RecordAsync(
            new RecordSwipeRequest(ActorId, TargetId, SwipeActionType.Like),
            CancellationToken.None);

        Assert.False(result.LikeRecorded);
        Assert.False(result.MatchCreated);
        Assert.Null(result.MatchId);
        Assert.True(result.Blocked);
    }

    /// <summary>Pass swipes bypass the block check entirely (nothing is persisted anyway).</summary>
    [Fact]
    public async Task Pass_WhenBlocked_ReturnsNormalPassResult()
    {
        RecordSwipeService svc = BuildBlocked();

        RecordSwipeResult result = await svc.RecordAsync(
            new RecordSwipeRequest(ActorId, TargetId, SwipeActionType.Pass),
            CancellationToken.None);

        Assert.False(result.LikeRecorded);
        Assert.False(result.MatchCreated);
        Assert.False(result.Blocked);
    }
}
