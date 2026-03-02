namespace Lunara.Api.Messaging;

/// <summary>JSON body for the <c>POST /v1/messaging/send</c> endpoint.</summary>
internal sealed record SendMessageBody(Guid MatchId, Guid RecipientUserId, string? Text);
