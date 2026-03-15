namespace Lunara.Api.Moderation;

/// <summary>JSON body for the <c>POST /v1/moderation/block</c> endpoint.</summary>
internal sealed record BlockBody(Guid BlockedUserId);
