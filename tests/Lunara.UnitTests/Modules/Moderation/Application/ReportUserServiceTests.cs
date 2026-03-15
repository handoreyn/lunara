using Lunara.Moderation.Application.DTOs;
using Lunara.Moderation.Application.UseCases;
using Lunara.UnitTests.Modules.Moderation.Fakes;

namespace Lunara.UnitTests.Modules.Moderation.Application;

/// <summary>Unit tests for <see cref="ReportUserService"/>.</summary>
public sealed class ReportUserServiceTests
{
    private static readonly DateTimeOffset FixedNow =
        new(2026, 3, 15, 12, 0, 0, TimeSpan.Zero);

    private static readonly Guid UserA = new("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid UserB = new("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    private sealed record Fixtures(
        ReportUserService Svc,
        FakeReportRepository ReportRepo,
        FakeUnitOfWork Uow);

    private static Fixtures Build()
    {
        FakeReportRepository reportRepo = new();
        FakeUnitOfWork uow = new();
        FakeClock clock = new(FixedNow);
        ReportUserService svc = new(reportRepo, uow, clock);
        return new Fixtures(svc, reportRepo, uow);
    }

    // ── Self-report guard ──────────────────────────────────────────────────

    [Fact]
    public async Task ReportAsync_SameUser_Throws_ArgumentException()
    {
        Fixtures f = Build();
        ReportUserRequest request = new(UserA, UserA, "spam", null);

        await Assert.ThrowsAsync<ArgumentException>(
            () => f.Svc.ReportAsync(request, CancellationToken.None));
    }

    // ── Reason validation ──────────────────────────────────────────────────

    [Fact]
    public async Task ReportAsync_EmptyReason_Throws_ArgumentException()
    {
        Fixtures f = Build();
        ReportUserRequest request = new(UserA, UserB, "  ", null);

        await Assert.ThrowsAsync<ArgumentException>(
            () => f.Svc.ReportAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task ReportAsync_ReasonTooLong_Throws_ArgumentException()
    {
        Fixtures f = Build();
        string longReason = new('x', 201);
        ReportUserRequest request = new(UserA, UserB, longReason, null);

        await Assert.ThrowsAsync<ArgumentException>(
            () => f.Svc.ReportAsync(request, CancellationToken.None));
    }

    // ── Happy path ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ReportAsync_ValidRequest_ReturnsReportId_AndSavesOnce()
    {
        Fixtures f = Build();
        ReportUserRequest request = new(UserA, UserB, "harassment", "Details here");

        ReportUserResult result = await f.Svc.ReportAsync(request, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.ReportId);
        Assert.Equal(1, f.ReportRepo.AddCallCount);
        Assert.Equal(1, f.Uow.SaveCallCount);
    }
}
