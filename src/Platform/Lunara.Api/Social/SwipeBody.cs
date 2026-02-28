namespace Lunara.Api.Social;

/// <summary>JSON body for the <c>POST /v1/social/swipe</c> endpoint.</summary>
internal sealed record SwipeBody(Guid TargetUserId, string? Action);
