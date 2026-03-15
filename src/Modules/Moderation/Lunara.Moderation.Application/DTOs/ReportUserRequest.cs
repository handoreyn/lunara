using Lunara.Moderation.Domain.ValueObjects;

namespace Lunara.Moderation.Application.DTOs;

/// <summary>Input data for the report-user use case.</summary>
/// <param name="ReporterUserId">The user filing the report.</param>
/// <param name="TargetUserId">The user being reported.</param>
/// <param name="Reason">A short description of the reason.</param>
/// <param name="Details">Optional additional details.</param>
public sealed record ReportUserRequest(
    UserId ReporterUserId,
    UserId TargetUserId,
    string Reason,
    string? Details);
