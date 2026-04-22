using Lunara.Moderation.Domain.Entities;
using Lunara.Moderation.Domain.ValueObjects;

namespace Lunara.UnitTests.Modules.Moderation.Domain;

/// <summary>Unit tests for the Moderation domain entities and value objects.</summary>
public sealed class ModerationDomainTests
{
    // ── BlockId ────────────────────────────────────────────────────────────

    [Fact]
    public void BlockId_With_Empty_Guid_Throws_ArgumentException()
    {
        ArgumentException ex = Assert.Throws<ArgumentException>(() => _ = new BlockId(Guid.Empty));
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
        ArgumentException ex = Assert.Throws<ArgumentException>(() => _ = new ReportId(Guid.Empty));
        Assert.Contains("ReportId must not be empty", ex.Message, StringComparison.Ordinal);
    }

    // ── UserId ─────────────────────────────────────────────────────────────

    [Fact]
    public void UserId_With_Empty_Guid_Throws_ArgumentException()
    {
        ArgumentException ex = Assert.Throws<ArgumentException>(() => _ = new UserId(Guid.Empty));
        Assert.Contains("UserId must not be empty", ex.Message, StringComparison.Ordinal);
    }

    // ── Block entity ───────────────────────────────────────────────────────

    [Fact]
    public void Block_Create_SameUser_Throws_ArgumentException()
    {
        UserId userId = new(Guid.NewGuid());
        DateTimeOffset now = DateTimeOffset.UtcNow;

        ArgumentException ex = Assert.Throws<ArgumentException>(
            () => Block.Create(userId, userId, now));

        Assert.Contains("cannot block themselves", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Block_Create_DifferentUsers_Succeeds()
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
    public void Block_Reconstitute_Restores_All_Properties()
    {
        BlockId id = new(Guid.NewGuid());
        UserId blocker = new(Guid.NewGuid());
        UserId blocked = new(Guid.NewGuid());
        DateTimeOffset now = DateTimeOffset.UtcNow;

        Block block = Block.Reconstitute(id, blocker, blocked, now);

        Assert.Equal(id, block.Id);
        Assert.Equal(blocker, block.BlockerUserId);
        Assert.Equal(blocked, block.BlockedUserId);
        Assert.Equal(now, block.CreatedAtUtc);
    }

    // ── Report entity ──────────────────────────────────────────────────────

    [Fact]
    public void Report_Create_SameUser_Throws_ArgumentException()
    {
        UserId userId = new(Guid.NewGuid());
        DateTimeOffset now = DateTimeOffset.UtcNow;

        ArgumentException ex = Assert.Throws<ArgumentException>(
            () => Report.Create(userId, userId, "spam", null, now));

        Assert.Contains("cannot report themselves", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Report_Create_EmptyReason_Throws_ArgumentException()
    {
        UserId reporter = new(Guid.NewGuid());
        UserId target = new(Guid.NewGuid());
        DateTimeOffset now = DateTimeOffset.UtcNow;

        ArgumentException ex = Assert.Throws<ArgumentException>(
            () => Report.Create(reporter, target, "   ", null, now));

        Assert.Contains("must not be empty", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Report_Create_ReasonTooLong_Throws_ArgumentException()
    {
        UserId reporter = new(Guid.NewGuid());
        UserId target = new(Guid.NewGuid());
        string longReason = new('x', Report.MaxReasonLength + 1);
        DateTimeOffset now = DateTimeOffset.UtcNow;

        ArgumentException ex = Assert.Throws<ArgumentException>(
            () => Report.Create(reporter, target, longReason, null, now));

        Assert.Contains($"must not exceed {Report.MaxReasonLength}", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Report_Create_Valid_Succeeds_And_TrimsReason()
    {
        UserId reporter = new(Guid.NewGuid());
        UserId target = new(Guid.NewGuid());
        DateTimeOffset now = DateTimeOffset.UtcNow;

        Report report = Report.Create(reporter, target, "  spam  ", "some details", now);

        Assert.Equal(reporter, report.ReporterUserId);
        Assert.Equal(target, report.TargetUserId);
        Assert.Equal("spam", report.Reason);
        Assert.Equal("some details", report.Details);
        Assert.Equal(now, report.CreatedAtUtc);
        Assert.NotEqual(Guid.Empty, report.Id.Value);
    }

    [Fact]
    public void Report_Create_MaxLengthReason_Succeeds()
    {
        UserId reporter = new(Guid.NewGuid());
        UserId target = new(Guid.NewGuid());
        string maxReason = new('x', Report.MaxReasonLength);

        Report report = Report.Create(reporter, target, maxReason, null, DateTimeOffset.UtcNow);

        Assert.Equal(maxReason, report.Reason);
    }
}
