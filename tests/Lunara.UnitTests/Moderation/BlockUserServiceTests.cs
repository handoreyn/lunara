using Lunara.Moderation.Application.DTOs;
using Lunara.Moderation.Application.UseCases;
using Lunara.Moderation.Domain.ValueObjects;
using Lunara.UnitTests.Moderation.Fakes;

namespace Lunara.UnitTests.Moderation;

/// <summary>Unit tests for <see cref="BlockUserService"/>.</summary>
public sealed class BlockUserServiceTests
{
    private static readonly DateTimeOffset FixedNow = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly UserId BlockerUserId = new(new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
    private static readonly UserId BlockedUserId = new(new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));

    private static (BlockUserService Svc, FakeBlockRepository Blocks, FakeModerationUnitOfWork Uow)
        Build(FakeBlockRepository? blocks = null)
    {
        FakeBlockRepository blockRepo = blocks ?? new FakeBlockRepository();
        FakeModerationUnitOfWork uow = new();
        FakeModerationClock clock = new(FixedNow);
        BlockUserService svc = new(blockRepo, uow, clock);
        return (svc, blockRepo, uow);
    }

    /// <summary>Blocking a distinct user creates a new block and saves once.</summary>
    [Fact]
    public async Task Block_NewPair_CreatesBlock_SavesOnce()
    {
        (BlockUserService svc, FakeBlockRepository blocks, FakeModerationUnitOfWork uow) = Build();

        BlockUserResult result = await svc.BlockAsync(
            new BlockUserRequest(BlockerUserId, BlockedUserId),
            CancellationToken.None);

        Assert.True(result.BlockCreated);
        Assert.Equal(1, blocks.AddCallCount);
        Assert.Equal(1, uow.SaveCallCount);
    }

    /// <summary>Blocking a user a second time is a no-op — nothing is persisted.</summary>
    [Fact]
    public async Task Block_DuplicatePair_IsNoOp_NothingPersisted()
    {
        FakeBlockRepository blocks = new();
        blocks.SeedBlock(BlockerUserId, BlockedUserId);
        (BlockUserService svc, _, FakeModerationUnitOfWork uow) = Build(blocks);

        BlockUserResult result = await svc.BlockAsync(
            new BlockUserRequest(BlockerUserId, BlockedUserId),
            CancellationToken.None);

        Assert.False(result.BlockCreated);
        Assert.Equal(0, uow.SaveCallCount);
    }

    /// <summary>Blocking oneself throws <see cref="ArgumentException"/>.</summary>
    [Fact]
    public async Task Block_SameUser_Throws()
    {
        (BlockUserService svc, _, _) = Build();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            svc.BlockAsync(
                new BlockUserRequest(BlockerUserId, BlockerUserId),
                CancellationToken.None));
    }
}
