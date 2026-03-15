using Lunara.Moderation.Domain.Entities;
using Lunara.Moderation.Domain.ValueObjects;

namespace Lunara.UnitTests.Moderation;

/// <summary>Unit tests for the Moderation domain entities.</summary>
public sealed class ModerationDomainTests
{
    private static readonly UserId UserA = new(new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
    private static readonly UserId UserB = new(new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));
    private static readonly DateTimeOffset FixedNow = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

    // ── Block entity ────────────────────────────────────────────────────────

    /// <summary>Creating a block with distinct users succeeds and sets all properties.</summary>
    [Fact]
    public void Block_Create_DifferentUsers_Succeeds()
    {
        Block block = Block.Create(UserA, UserB, FixedNow);

        Assert.Equal(UserA, block.BlockerUserId);
        Assert.Equal(UserB, block.BlockedUserId);
        Assert.Equal(FixedNow, block.CreatedAtUtc);
        Assert.NotEqual(Guid.Empty, block.Id.Value);
    }

    /// <summary>Blocking oneself throws <see cref="ArgumentException"/>.</summary>
    [Fact]
    public void Block_Create_SameUser_Throws()
    {
        Assert.Throws<ArgumentException>(() => Block.Create(UserA, UserA, FixedNow));
    }

    // ── Report entity ───────────────────────────────────────────────────────

    /// <summary>Creating a report with valid data succeeds and sets all properties.</summary>
    [Fact]
    public void Report_Create_ValidData_Succeeds()
    {
        Report report = Report.Create(UserA, UserB, "Spam", "Details here", FixedNow);

        Assert.Equal(UserA, report.ReporterUserId);
        Assert.Equal(UserB, report.TargetUserId);
        Assert.Equal("Spam", report.Reason);
        Assert.Equal("Details here", report.Details);
        Assert.Equal(FixedNow, report.CreatedAtUtc);
        Assert.NotEqual(Guid.Empty, report.Id.Value);
    }

    /// <summary>Creating a report with <c>null</c> details stores <c>null</c>.</summary>
    [Fact]
    public void Report_Create_NullDetails_Succeeds()
    {
        Report report = Report.Create(UserA, UserB, "Harassment", null, FixedNow);

        Assert.Null(report.Details);
    }

    /// <summary>Reporting oneself throws <see cref="ArgumentException"/>.</summary>
    [Fact]
    public void Report_Create_SameUser_Throws()
    {
        Assert.Throws<ArgumentException>(() => Report.Create(UserA, UserA, "Spam", null, FixedNow));
    }

    /// <summary>An empty reason throws <see cref="ArgumentException"/>.</summary>
    [Fact]
    public void Report_Create_EmptyReason_Throws()
    {
        Assert.Throws<ArgumentException>(() => Report.Create(UserA, UserB, "", null, FixedNow));
    }

    /// <summary>A whitespace-only reason throws <see cref="ArgumentException"/>.</summary>
    [Fact]
    public void Report_Create_WhitespaceReason_Throws()
    {
        Assert.Throws<ArgumentException>(() => Report.Create(UserA, UserB, "   ", null, FixedNow));
    }

    /// <summary>A reason exceeding the maximum length throws <see cref="ArgumentException"/>.</summary>
    [Fact]
    public void Report_Create_TooLongReason_Throws()
    {
        string tooLong = new('x', Report.MaxReasonLength + 1);
        Assert.Throws<ArgumentException>(() => Report.Create(UserA, UserB, tooLong, null, FixedNow));
    }

    /// <summary>A reason equal to the maximum length is accepted.</summary>
    [Fact]
    public void Report_Create_MaxLengthReason_Succeeds()
    {
        string maxLength = new('x', Report.MaxReasonLength);
        Report report = Report.Create(UserA, UserB, maxLength, null, FixedNow);

        Assert.Equal(maxLength, report.Reason);
    }

    // ── BlockId / ReportId ──────────────────────────────────────────────────

    /// <summary>Empty <see cref="BlockId"/> throws <see cref="ArgumentException"/>.</summary>
    [Fact]
    public void BlockId_EmptyGuid_Throws()
    {
        Assert.Throws<ArgumentException>(() => new BlockId(Guid.Empty));
    }

    /// <summary>Empty <see cref="ReportId"/> throws <see cref="ArgumentException"/>.</summary>
    [Fact]
    public void ReportId_EmptyGuid_Throws()
    {
        Assert.Throws<ArgumentException>(() => new ReportId(Guid.Empty));
    }

    /// <summary>Empty <see cref="UserId"/> throws <see cref="ArgumentException"/>.</summary>
    [Fact]
    public void UserId_EmptyGuid_Throws()
    {
        Assert.Throws<ArgumentException>(() => new UserId(Guid.Empty));
    }
}
