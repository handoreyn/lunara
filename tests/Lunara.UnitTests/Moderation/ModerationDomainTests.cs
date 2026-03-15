using Lunara.Moderation.Domain.Entities;
using Lunara.Moderation.Domain.ValueObjects;

namespace Lunara.UnitTests.Moderation;

/// <summary>Unit tests for Moderation domain entities and value objects.</summary>
public sealed class ModerationDomainTests
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

    // ── BlockId ────────────────────────────────────────────────────────────

    [Fact]
    public void BlockId_With_Empty_Guid_Throws_ArgumentException()
    {
        ArgumentException ex = Assert.Throws<ArgumentException>(
            () => _ = new BlockId(Guid.Empty));

        Assert.Contains("BlockId must not be empty", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void BlockId_With_Valid_Guid_Succeeds()
    {
        Guid guid = Guid.NewGuid();
        BlockId id = new(guid);
        Assert.Equal(guid, id.Value);
    }

    // ── ReportId ───────────────────────────────────────────────────────────

    [Fact]
    public void ReportId_With_Empty_Guid_Throws_ArgumentException()
    {
        ArgumentException ex = Assert.Throws<ArgumentException>(
            () => _ = new ReportId(Guid.Empty));

        Assert.Contains("ReportId must not be empty", ex.Message, StringComparison.Ordinal);
    }

    // ── Block entity ───────────────────────────────────────────────────────

    [Fact]
    public void Block_Create_With_Different_Users_Succeeds()
    {
        UserId blocker = new(Guid.NewGuid());
        UserId blocked = new(Guid.NewGuid());
        DateTimeOffset now = DateTimeOffset.UtcNow;

        Block block = Block.Create(blocker, blocked, now);

        Assert.Equal(blocker, block.BlockerUserId);
        Assert.Equal(blocked, block.BlockedUserId);
        Assert.Equal(now, block.CreatedAtUtc);
        Assert.NotEqual(Guid.Empty, block.Id.Value);
    }

    [Fact]
    public void Block_Create_Same_User_Throws_ArgumentException()
    {
        UserId user = new(Guid.NewGuid());

        ArgumentException ex = Assert.Throws<ArgumentException>(
            () => Block.Create(user, user, DateTimeOffset.UtcNow));

        Assert.Contains("cannot block themselves", ex.Message, StringComparison.Ordinal);
    }

    // ── Report entity ──────────────────────────────────────────────────────

    [Fact]
    public void Report_Create_With_Valid_Data_Succeeds()
    {
        UserId reporter = new(Guid.NewGuid());
        UserId target = new(Guid.NewGuid());
        DateTimeOffset now = DateTimeOffset.UtcNow;

        Report report = Report.Create(reporter, target, "Spam", "Details here", now);

        Assert.Equal(reporter, report.ReporterUserId);
        Assert.Equal(target, report.TargetUserId);
        Assert.Equal("Spam", report.Reason);
        Assert.Equal("Details here", report.Details);
        Assert.Equal(now, report.CreatedAtUtc);
        Assert.NotEqual(Guid.Empty, report.Id.Value);
    }

    [Fact]
    public void Report_Create_Same_User_Throws_ArgumentException()
    {
        UserId user = new(Guid.NewGuid());

        ArgumentException ex = Assert.Throws<ArgumentException>(
            () => Report.Create(user, user, "Spam", null, DateTimeOffset.UtcNow));

        Assert.Contains("cannot report themselves", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Report_Create_Empty_Reason_Throws_ArgumentException()
    {
        UserId reporter = new(Guid.NewGuid());
        UserId target = new(Guid.NewGuid());

        Assert.Throws<ArgumentException>(
            () => Report.Create(reporter, target, string.Empty, null, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Report_Create_Reason_Too_Long_Throws_ArgumentException()
    {
        UserId reporter = new(Guid.NewGuid());
        UserId target = new(Guid.NewGuid());
        string longReason = new('x', Report.MaxReasonLength + 1);

        ArgumentException ex = Assert.Throws<ArgumentException>(
            () => Report.Create(reporter, target, longReason, null, DateTimeOffset.UtcNow));

        Assert.Contains($"{Report.MaxReasonLength} characters", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Report_Create_Reason_At_Max_Length_Succeeds()
    {
        UserId reporter = new(Guid.NewGuid());
        UserId target = new(Guid.NewGuid());
        string maxReason = new('x', Report.MaxReasonLength);

        Report report = Report.Create(reporter, target, maxReason, null, DateTimeOffset.UtcNow);

        Assert.Equal(Report.MaxReasonLength, report.Reason.Length);
    }

    [Fact]
    public void Report_Create_Null_Details_Is_Allowed()
    {
        UserId reporter = new(Guid.NewGuid());
        UserId target = new(Guid.NewGuid());

        Report report = Report.Create(reporter, target, "Harassment", null, DateTimeOffset.UtcNow);

        Assert.Null(report.Details);
    }
}
