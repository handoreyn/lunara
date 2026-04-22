namespace Lunara.Moderation.Application.DTOs;

/// <summary>Input data for the report-user use case.</summary>
/// <param name="ReporterUserId">The identifier of the user filing the report.</param>
/// <param name="TargetUserId">The identifier of the user being reported.</param>
/// <param name="Reason">A short description of the reason for the report (1–200 characters).</param>
/// <param name="Details">Optional additional details. May be <c>null</c>.</param>
public sealed record ReportUserRequest(
    Guid ReporterUserId,
    Guid TargetUserId,
    string Reason,
    string? Details);
