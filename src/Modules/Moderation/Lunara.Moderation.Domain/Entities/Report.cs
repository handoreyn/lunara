using Lunara.Moderation.Domain.ValueObjects;

namespace Lunara.Moderation.Domain.Entities;

/// <summary>
/// Represents a report filed by one user against another.
/// A user cannot report themselves, and the reason must not exceed
/// <see cref="MaxReasonLength"/> characters.
/// </summary>
public sealed class Report
{
    /// <summary>Maximum allowed length for the <see cref="Reason"/> field.</summary>
    public const int MaxReasonLength = 1000;

    /// <summary>Maximum allowed length for the <see cref="Details"/> field.</summary>
    public const int MaxDetailsLength = 1000;

    /// <summary>Gets the unique identifier of this report.</summary>
    public ReportId Id { get; }

    /// <summary>Gets the identifier of the user who filed the report.</summary>
    public UserId ReporterUserId { get; }

    /// <summary>Gets the identifier of the user who was reported.</summary>
    public UserId TargetUserId { get; }

    /// <summary>Gets the human-readable reason for the report.</summary>
    public string Reason { get; }

    /// <summary>Gets optional additional details provided by the reporter.</summary>
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
    /// Creates a new <see cref="Report"/> filed by a user against another.
    /// </summary>
    /// <param name="reporterUserId">The user filing the report.</param>
    /// <param name="targetUserId">The user being reported.</param>
    /// <param name="reason">A short description of the reason for the report. Must not exceed <see cref="MaxReasonLength"/> characters.</param>
    /// <param name="details">Optional additional details. Must not exceed <see cref="MaxDetailsLength"/> characters when provided.</param>
    /// <param name="createdAtUtc">The UTC timestamp of the report action.</param>
    /// <returns>A new <see cref="Report"/> instance.</returns>
    /// <exception cref="ArgumentException">
    ///   Thrown when <paramref name="reporterUserId"/> equals <paramref name="targetUserId"/>,
    ///   or when <paramref name="reason"/> exceeds <see cref="MaxReasonLength"/> characters,
    ///   or when <paramref name="reason"/> is null or empty,
    ///   or when <paramref name="details"/> exceeds <see cref="MaxDetailsLength"/> characters.
    /// </exception>
    public static Report Create(
        UserId reporterUserId,
        UserId targetUserId,
        string reason,
        string? details,
        DateTimeOffset createdAtUtc)
    {
        if (reporterUserId == targetUserId)
        {
            throw new ArgumentException(
                "A user cannot report themselves.",
                nameof(targetUserId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(reason, nameof(reason));

        if (reason.Length > MaxReasonLength)
        {
            throw new ArgumentException(
                $"Reason must not exceed {MaxReasonLength} characters.",
                nameof(reason));
        }

        if (details is not null && details.Length > MaxDetailsLength)
        {
            throw new ArgumentException(
                $"Details must not exceed {MaxDetailsLength} characters.",
                nameof(details));
        }

        ReportId id = new(Guid.NewGuid());
        return new Report(id, reporterUserId, targetUserId, reason, details, createdAtUtc);
    }
}
