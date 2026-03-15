using Lunara.Moderation.Application.DTOs;
using Lunara.Moderation.Application.UseCases;
using Lunara.Moderation.Domain.ValueObjects;
using Lunara.UnitTests.Moderation.Fakes;

namespace Lunara.UnitTests.Moderation;

/// <summary>Unit tests for <see cref="ReportUserService"/>.</summary>
public sealed class ReportUserServiceTests
{
    private static readonly DateTimeOffset FixedNow =
        new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static readonly UserId ReporterUserId = new(new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
    private static readonly UserId TargetUserId = new(new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));

    private static (ReportUserService Svc, FakeReportRepository Repo, FakeModerationUnitOfWork Uow) Build()
    {
        FakeReportRepository reportRepo = new();
        FakeModerationUnitOfWork uow = new();
        FakeModerationClock clock = new(FixedNow);
        ReportUserService svc = new(reportRepo, uow, clock);
        return (svc, reportRepo, uow);
    }

    /// <summary>Reporting a user persists the report and saves once.</summary>
    [Fact]
    public async Task Report_ValidRequest_Persists_And_SavesOnce()
    {
        (ReportUserService svc, FakeReportRepository repo, FakeModerationUnitOfWork uow) = Build();

        ReportUserResult result = await svc.ReportAsync(
            new ReportUserRequest(ReporterUserId, TargetUserId, "Spam", "Lots of ads"),
            CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.ReportId);
        Assert.Single(repo.Reports);
        Assert.Equal(1, uow.SaveCallCount);
    }

    /// <summary>Reporting oneself throws <see cref="ArgumentException"/>.</summary>
    [Fact]
    public async Task Report_SameUser_Throws_ArgumentException()
    {
        (ReportUserService svc, _, _) = Build();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            svc.ReportAsync(
                new ReportUserRequest(ReporterUserId, ReporterUserId, "Spam", null),
                CancellationToken.None));
    }

    /// <summary>Empty reason throws <see cref="ArgumentException"/>.</summary>
    [Fact]
    public async Task Report_EmptyReason_Throws_ArgumentException()
    {
        (ReportUserService svc, _, _) = Build();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            svc.ReportAsync(
                new ReportUserRequest(ReporterUserId, TargetUserId, string.Empty, null),
                CancellationToken.None));
    }

    /// <summary>Reason that exceeds the maximum length throws <see cref="ArgumentException"/>.</summary>
    [Fact]
    public async Task Report_ReasonTooLong_Throws_ArgumentException()
    {
        (ReportUserService svc, _, _) = Build();
        string longReason = new('x', Lunara.Moderation.Domain.Entities.Report.MaxReasonLength + 1);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            svc.ReportAsync(
                new ReportUserRequest(ReporterUserId, TargetUserId, longReason, null),
                CancellationToken.None));
    }

    /// <summary>Null request throws <see cref="ArgumentNullException"/>.</summary>
    [Fact]
    public async Task Report_NullRequest_Throws_ArgumentNullException()
    {
        (ReportUserService svc, _, _) = Build();

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            svc.ReportAsync(null!, CancellationToken.None));
    }

    /// <summary>Report with null details is persisted successfully.</summary>
    [Fact]
    public async Task Report_NullDetails_Succeeds()
    {
        (ReportUserService svc, FakeReportRepository repo, _) = Build();

        await svc.ReportAsync(
            new ReportUserRequest(ReporterUserId, TargetUserId, "Harassment", null),
            CancellationToken.None);

        Assert.Single(repo.Reports);
        Assert.Null(repo.Reports[0].Details);
    }
}
