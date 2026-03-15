namespace Lunara.Moderation.Application.DTOs;

/// <summary>Output of the <see cref="UseCases.ReportUserService.ReportAsync"/> use-case.</summary>
/// <param name="ReportCreated"><c>true</c> when the report was successfully persisted.</param>
public sealed record ReportUserResult(bool ReportCreated);
