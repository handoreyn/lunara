namespace Lunara.Moderation.Infrastructure.Persistence;

/// <summary>
/// EF Core persistence row for a <see cref="Domain.Entities.Report"/> aggregate.
/// All columns are raw primitives; no domain value objects cross the persistence boundary.
/// </summary>
public sealed class ReportEntity
{
    /// <summary>Gets or sets the unique identifier of the report.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the identifier of the user who filed the report.</summary>
    public Guid ReporterUserId { get; set; }

    /// <summary>Gets or sets the identifier of the user being reported.</summary>
    public Guid TargetUserId { get; set; }

    /// <summary>Gets or sets the reason for the report.</summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional additional details, or <c>null</c> if none.</summary>
    public string? Details { get; set; }

    /// <summary>Gets or sets the UTC timestamp at which this report was created.</summary>
    public DateTimeOffset CreatedAtUtc { get; set; }
}
