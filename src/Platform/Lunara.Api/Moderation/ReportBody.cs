namespace Lunara.Api.Moderation;

/// <summary>Request body for the report-user endpoint.</summary>
internal sealed record ReportBody(Guid TargetUserId, string Reason, string? Details);
