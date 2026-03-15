using Lunara.Moderation.Domain.ValueObjects;

namespace Lunara.Moderation.Application.DTOs;

/// <summary>Input for the <see cref="UseCases.ReportUserService.ReportAsync"/> use-case.</summary>
/// <param name="ReporterUserId">The user submitting the report.</param>
/// <param name="TargetUserId">The user being reported.</param>
/// <param name="Reason">A short description of the report reason (1–500 characters).</param>
/// <param name="Details">Optional free-text details. May be <c>null</c>.</param>
public sealed record ReportUserRequest(
    UserId ReporterUserId,
    UserId TargetUserId,
    string Reason,
    string? Details);
