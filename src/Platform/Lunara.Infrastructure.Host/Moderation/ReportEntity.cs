namespace Lunara.Infrastructure.Host.Moderation;

/// <summary>EF Core entity for the <c>moderation_reports</c> table.</summary>
internal sealed class ReportEntity
{
    /// <summary>Gets or sets the primary key.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the identifier of the user who submitted the report.</summary>
    public Guid ReporterUserId { get; set; }

    /// <summary>Gets or sets the identifier of the user being reported.</summary>
    public Guid TargetUserId { get; set; }

    /// <summary>Gets or sets the short reason for the report.</summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>Gets or sets optional free-text details, or <c>null</c> when not supplied.</summary>
    public string? Details { get; set; }

    /// <summary>Gets or sets the UTC timestamp at which the report was created.</summary>
    public DateTimeOffset CreatedAtUtc { get; set; }
}
