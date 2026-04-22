namespace Lunara.Api.Moderation;

/// <summary>Request body for the block-user endpoint.</summary>
internal sealed record BlockBody(Guid TargetUserId);
