using Lunara.Moderation.Application.DTOs;
using Lunara.Moderation.Application.UseCases;
using Lunara.Moderation.Domain.ValueObjects;
using Lunara.UnitTests.Moderation.Fakes;

namespace Lunara.UnitTests.Moderation;

/// <summary>Unit tests for <see cref="ReportUserService"/>.</summary>
public sealed class ReportUserServiceTests
{
    private static readonly DateTimeOffset FixedNow = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly UserId ReporterUserId = new(new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
    private static readonly UserId TargetUserId = new(new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));

    private static (ReportUserService Svc, FakeReportRepository Reports, FakeModerationUnitOfWork Uow)
        Build()
    {
        FakeReportRepository reportRepo = new();
        FakeModerationUnitOfWork uow = new();
        FakeModerationClock clock = new(FixedNow);
        ReportUserService svc = new(reportRepo, uow, clock);
        return (svc, reportRepo, uow);
    }

    /// <summary>Reporting another user with valid data persists the report and saves once.</summary>
    [Fact]
    public async Task Report_ValidData_PersistsReport_SavesOnce()
    {
        (ReportUserService svc, FakeReportRepository reports, FakeModerationUnitOfWork uow) = Build();

        ReportUserResult result = await svc.ReportAsync(
            new ReportUserRequest(ReporterUserId, TargetUserId, "Spam", null),
            CancellationToken.None);

        Assert.True(result.ReportCreated);
        Assert.Single(reports.AddedReports);
        Assert.Equal(1, uow.SaveCallCount);
    }

    /// <summary>The report entity is created with the correct values.</summary>
    [Fact]
    public async Task Report_ValidData_EntityHasCorrectValues()
    {
        (ReportUserService svc, FakeReportRepository reports, _) = Build();

        await svc.ReportAsync(
            new ReportUserRequest(ReporterUserId, TargetUserId, "Harassment", "Some details"),
            CancellationToken.None);

        Lunara.Moderation.Domain.Entities.Report report = reports.AddedReports[0];
        Assert.Equal(ReporterUserId, report.ReporterUserId);
        Assert.Equal(TargetUserId, report.TargetUserId);
        Assert.Equal("Harassment", report.Reason);
        Assert.Equal("Some details", report.Details);
        Assert.Equal(FixedNow, report.CreatedAtUtc);
    }

    /// <summary>Reporting oneself propagates the domain exception.</summary>
    [Fact]
    public async Task Report_SameUser_Throws()
    {
        (ReportUserService svc, _, _) = Build();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            svc.ReportAsync(
                new ReportUserRequest(ReporterUserId, ReporterUserId, "Self", null),
                CancellationToken.None));
    }

    /// <summary>An empty reason propagates the domain exception.</summary>
    [Fact]
    public async Task Report_EmptyReason_Throws()
    {
        (ReportUserService svc, _, _) = Build();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            svc.ReportAsync(
                new ReportUserRequest(ReporterUserId, TargetUserId, "", null),
                CancellationToken.None));
    }
}
