using Lunara.Moderation.Application.DTOs;
using Lunara.Moderation.Application.UseCases;
using Lunara.Moderation.Domain.ValueObjects;
using Lunara.UnitTests.Moderation.Fakes;

namespace Lunara.UnitTests.Moderation;

/// <summary>Unit tests for <see cref="BlockUserService"/>.</summary>
public sealed class BlockUserServiceTests
{
    private static readonly DateTimeOffset FixedNow =
        new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static readonly UserId BlockerUserId = new(new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
    private static readonly UserId BlockedUserId = new(new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));

    private static (BlockUserService Svc, FakeBlockRepository Repo, FakeModerationUnitOfWork Uow)
        Build(FakeBlockRepository? repo = null)
    {
        FakeBlockRepository blockRepo = repo ?? new FakeBlockRepository();
        FakeModerationUnitOfWork uow = new();
        FakeModerationClock clock = new(FixedNow);
        BlockUserService svc = new(blockRepo, uow, clock);
        return (svc, blockRepo, uow);
    }

    /// <summary>Blocking a new user persists the block and saves once.</summary>
    [Fact]
    public async Task Block_NewBlock_Persists_And_SavesOnce()
    {
        (BlockUserService svc, FakeBlockRepository repo, FakeModerationUnitOfWork uow) = Build();

        BlockUserResult result = await svc.BlockAsync(
            new BlockUserRequest(BlockerUserId, BlockedUserId),
            CancellationToken.None);

        Assert.False(result.AlreadyBlocked);
        Assert.NotNull(result.BlockId);
        Assert.Single(repo.Blocks);
        Assert.Equal(1, uow.SaveCallCount);
    }

    /// <summary>A duplicate block is a no-op — not persisted, not saved.</summary>
    [Fact]
    public async Task Block_Duplicate_IsNoOp_NoSave()
    {
        FakeBlockRepository repo = new();
        repo.SeedBlock(BlockerUserId, BlockedUserId);
        (BlockUserService svc, _, FakeModerationUnitOfWork uow) = Build(repo);

        BlockUserResult result = await svc.BlockAsync(
            new BlockUserRequest(BlockerUserId, BlockedUserId),
            CancellationToken.None);

        Assert.True(result.AlreadyBlocked);
        Assert.Null(result.BlockId);
        Assert.Equal(0, uow.SaveCallCount);
    }

    /// <summary>Blocking oneself throws <see cref="ArgumentException"/>.</summary>
    [Fact]
    public async Task Block_SameUser_Throws_ArgumentException()
    {
        (BlockUserService svc, _, _) = Build();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            svc.BlockAsync(
                new BlockUserRequest(BlockerUserId, BlockerUserId),
                CancellationToken.None));
    }

    /// <summary>Null request throws <see cref="ArgumentNullException"/>.</summary>
    [Fact]
    public async Task Block_NullRequest_Throws_ArgumentNullException()
    {
        (BlockUserService svc, _, _) = Build();

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            svc.BlockAsync(null!, CancellationToken.None));
    }

    /// <summary>A successful block returns the correct block identifier.</summary>
    [Fact]
    public async Task Block_NewBlock_Returns_NonEmpty_BlockId()
    {
        (BlockUserService svc, _, _) = Build();

        BlockUserResult result = await svc.BlockAsync(
            new BlockUserRequest(BlockerUserId, BlockedUserId),
            CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.BlockId);
    }
}
