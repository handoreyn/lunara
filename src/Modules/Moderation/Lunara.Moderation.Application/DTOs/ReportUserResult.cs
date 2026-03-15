namespace Lunara.Moderation.Application.DTOs;

/// <summary>Result returned by the report-user use case.</summary>
/// <param name="ReportId">The identifier of the newly created report.</param>
public sealed record ReportUserResult(Guid ReportId);
