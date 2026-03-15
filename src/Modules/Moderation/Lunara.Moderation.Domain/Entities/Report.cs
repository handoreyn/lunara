using Lunara.Moderation.Domain.ValueObjects;

namespace Lunara.Moderation.Domain.Entities;

/// <summary>
/// Aggregate root representing a user reporting another user.
/// <para>
/// A user cannot report themselves.
/// The report <see cref="Reason"/> must be between 1 and <see cref="MaxReasonLength"/> characters.
/// </para>
/// </summary>
public sealed class Report
{
    /// <summary>Maximum allowed length for <see cref="Reason"/>.</summary>
    public const int MaxReasonLength = 500;

    /// <summary>Gets the unique identifier of this report.</summary>
    public ReportId Id { get; }

    /// <summary>Gets the identifier of the user who submitted the report.</summary>
    public UserId ReporterUserId { get; }

    /// <summary>Gets the identifier of the user being reported.</summary>
    public UserId TargetUserId { get; }

    /// <summary>Gets the short reason category for the report.</summary>
    public string Reason { get; }

    /// <summary>Gets optional free-text details provided by the reporter, or <c>null</c> when not supplied.</summary>
    public string? Details { get; }

    /// <summary>Gets the UTC timestamp at which the report was created.</summary>
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
    /// Creates a new <see cref="Report"/>.
    /// </summary>
    /// <param name="reporterUserId">The user submitting the report.</param>
    /// <param name="targetUserId">The user being reported.</param>
    /// <param name="reason">A short description of the report reason. Must be 1–500 characters.</param>
    /// <param name="details">Optional additional details. May be <c>null</c>.</param>
    /// <param name="createdAtUtc">The UTC timestamp of the report.</param>
    /// <returns>A new <see cref="Report"/> instance.</returns>
    /// <exception cref="ArgumentException">
    ///   Thrown when <paramref name="reporterUserId"/> equals <paramref name="targetUserId"/>,
    ///   or when <paramref name="reason"/> is null, empty, or exceeds <see cref="MaxReasonLength"/> characters.
    /// </exception>
    public static Report Create(
        UserId reporterUserId,
        UserId targetUserId,
        string reason,
        string? details,
        DateTimeOffset createdAtUtc)
    {
        return reporterUserId == targetUserId
            ? throw new ArgumentException(
                "A user cannot report themselves.",
                nameof(targetUserId))
            : string.IsNullOrWhiteSpace(reason)
                ? throw new ArgumentException(
                    "Reason must not be empty.",
                    nameof(reason))
                : reason.Length <= MaxReasonLength
                    ? new Report(
                        new ReportId(Guid.NewGuid()),
                        reporterUserId,
                        targetUserId,
                        reason,
                        details,
                        createdAtUtc)
                    : throw new ArgumentException(
                        $"Reason must not exceed {MaxReasonLength} characters.",
                        nameof(reason));
    }
}
