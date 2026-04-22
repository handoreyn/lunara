using Lunara.Moderation.Application.DTOs;
using Lunara.Moderation.Application.UseCases;
using Lunara.UnitTests.Modules.Moderation.Fakes;

namespace Lunara.UnitTests.Modules.Moderation.Application;

/// <summary>Unit tests for <see cref="BlockUserService"/>.</summary>
public sealed class BlockUserServiceTests
{
    private static readonly DateTimeOffset FixedNow =
        new(2026, 3, 15, 12, 0, 0, TimeSpan.Zero);

    private static readonly Guid UserA = new("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid UserB = new("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    private sealed record Fixtures(
        BlockUserService Svc,
        FakeBlockRepository BlockRepo,
        FakeUnitOfWork Uow);

    private static Fixtures Build()
    {
        FakeBlockRepository blockRepo = new();
        FakeUnitOfWork uow = new();
        FakeClock clock = new(FixedNow);
        BlockUserService svc = new(blockRepo, uow, clock);
        return new Fixtures(svc, blockRepo, uow);
    }

    // ── Self-block guard ───────────────────────────────────────────────────

    [Fact]
    public async Task BlockAsync_SameUser_Throws_ArgumentException()
    {
        Fixtures f = Build();
        BlockUserRequest request = new(UserA, UserA);

        await Assert.ThrowsAsync<ArgumentException>(
            () => f.Svc.BlockAsync(request, CancellationToken.None));
    }

    // ── Happy path ─────────────────────────────────────────────────────────

    [Fact]
    public async Task BlockAsync_NewBlock_ReturnsBlockId_AndSavesOnce()
    {
        Fixtures f = Build();
        BlockUserRequest request = new(UserA, UserB);

        BlockUserResult result = await f.Svc.BlockAsync(request, CancellationToken.None);

        Assert.False(result.AlreadyBlocked);
        Assert.NotNull(result.BlockId);
        Assert.NotEqual(Guid.Empty, result.BlockId!.Value);
        Assert.Equal(1, f.BlockRepo.AddCallCount);
        Assert.Equal(1, f.Uow.SaveCallCount);
    }

    // ── Duplicate is no-op ─────────────────────────────────────────────────

    [Fact]
    public async Task BlockAsync_DuplicateBlock_IsNoOp_ReturnsAlreadyBlocked()
    {
        Fixtures f = Build();
        BlockUserRequest request = new(UserA, UserB);

        // First block
        await f.Svc.BlockAsync(request, CancellationToken.None);

        // Second block — should be no-op
        BlockUserResult result = await f.Svc.BlockAsync(request, CancellationToken.None);

        Assert.True(result.AlreadyBlocked);
        Assert.Null(result.BlockId);
        Assert.Equal(1, f.BlockRepo.AddCallCount); // still only 1 add
        Assert.Equal(1, f.Uow.SaveCallCount);       // still only 1 save
    }
}
