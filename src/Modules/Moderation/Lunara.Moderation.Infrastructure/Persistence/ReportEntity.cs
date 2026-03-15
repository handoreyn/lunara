namespace Lunara.Moderation.Infrastructure.Persistence;

/// <summary>EF Core entity representing a row in the <c>moderation_reports</c> table.</summary>
internal sealed class ReportEntity
{
    /// <summary>Gets or sets the primary key.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the identifier of the user who filed the report.</summary>
    public Guid ReporterUserId { get; set; }

    /// <summary>Gets or sets the identifier of the user who was reported.</summary>
    public Guid TargetUserId { get; set; }

    /// <summary>Gets or sets the reason for the report.</summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>Gets or sets optional additional details.</summary>
    public string? Details { get; set; }

    /// <summary>Gets or sets the UTC timestamp at which the report was created.</summary>
    public DateTimeOffset CreatedAtUtc { get; set; }
}
