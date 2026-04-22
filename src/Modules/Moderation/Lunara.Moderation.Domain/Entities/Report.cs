using Lunara.Moderation.Domain.ValueObjects;

namespace Lunara.Moderation.Domain.Entities;

/// <summary>
/// Aggregate root representing a report filed by one user against another.
/// </summary>
public sealed class Report
{
    /// <summary>Maximum allowed length for the <see cref="Reason"/> string.</summary>
    public const int MaxReasonLength = 200;

    /// <summary>Gets the unique identifier of this report.</summary>
    public ReportId Id { get; }

    /// <summary>Gets the identifier of the user who filed the report.</summary>
    public UserId ReporterUserId { get; }

    /// <summary>Gets the identifier of the user being reported.</summary>
    public UserId TargetUserId { get; }

    /// <summary>Gets the short reason describing why the report was filed.</summary>
    public string Reason { get; }

    /// <summary>Gets optional additional details about the report, or <c>null</c> if none were provided.</summary>
    public string? Details { get; }

    /// <summary>Gets the UTC timestamp at which this report was created.</summary>
    public DateTimeOffset CreatedAtUtc { get; }

    private Report(
        ReportId id,
        UserId reporterUserId,
        UserId targetUserId,
        string reason,
        string? details,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        ReporterUserId = reporterUserId;
        TargetUserId = targetUserId;
        Reason = reason;
        Details = details;
        CreatedAtUtc = createdAtUtc;
    }

    /// <summary>
    /// Creates a new <see cref="Report"/> filed by one user against another.
    /// </summary>
    /// <param name="reporterUserId">The user filing the report.</param>
    /// <param name="targetUserId">The user being reported.</param>
    /// <param name="reason">
    ///   A short description of the reason for the report.
    ///   Must be between 1 and <see cref="MaxReasonLength"/> characters.
    /// </param>
    /// <param name="details">Optional additional details. May be <c>null</c>.</param>
    /// <param name="nowUtc">The current UTC timestamp.</param>
    /// <returns>A new <see cref="Report"/> instance.</returns>
    /// <exception cref="ArgumentException">
    ///   Thrown when <paramref name="reporterUserId"/> equals <paramref name="targetUserId"/>,
    ///   or when <paramref name="reason"/> is empty or exceeds <see cref="MaxReasonLength"/> characters.
    /// </exception>
    public static Report Create(
        UserId reporterUserId,
        UserId targetUserId,
        string reason,
        string? details,
        DateTimeOffset nowUtc)
    {
        if (reporterUserId == targetUserId)
        {
            throw new ArgumentException(
                "A user cannot report themselves.",
                nameof(targetUserId));
        }

        string trimmedReason = reason?.Trim() ?? string.Empty;

        return trimmedReason.Length == 0
            ? throw new ArgumentException("Reason must not be empty.", nameof(reason))
            : trimmedReason.Length > MaxReasonLength
                ? throw new ArgumentException(
                    $"Reason must not exceed {MaxReasonLength} characters.",
                    nameof(reason))
                : new Report(
                    new ReportId(Guid.NewGuid()),
                    reporterUserId,
                    targetUserId,
                    trimmedReason,
                    details,
                    nowUtc);
    }

    /// <summary>
    /// Reconstitutes a <see cref="Report"/> from persisted state.
    /// </summary>
    /// <param name="id">The report identifier.</param>
    /// <param name="reporterUserId">The identifier of the reporting user.</param>
    /// <param name="targetUserId">The identifier of the reported user.</param>
    /// <param name="reason">The reason for the report.</param>
    /// <param name="details">Optional additional details.</param>
    /// <param name="createdAtUtc">The UTC timestamp at which the report was created.</param>
    /// <returns>A reconstituted <see cref="Report"/> instance.</returns>
    public static Report Reconstitute(
        ReportId id,
        UserId reporterUserId,
        UserId targetUserId,
        string reason,
        string? details,
        DateTimeOffset createdAtUtc)
    {
        return new Report(id, reporterUserId, targetUserId, reason, details, createdAtUtc);
    }
}
