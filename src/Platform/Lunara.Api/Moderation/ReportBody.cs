namespace Lunara.Api.Moderation;

/// <summary>JSON body for the <c>POST /v1/moderation/report</c> endpoint.</summary>
internal sealed record ReportBody(Guid TargetUserId, string? Reason, string? Details);
