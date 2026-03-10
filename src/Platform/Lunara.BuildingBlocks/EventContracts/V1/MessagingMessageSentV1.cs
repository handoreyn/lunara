namespace Lunara.BuildingBlocks.EventContracts.V1;

/// <summary>
/// Integration event emitted when a message is sent within the Messaging module.
/// </summary>
/// <param name="ConversationId">Unique identifier of the conversation the message belongs to.</param>
/// <param name="MessageId">Unique identifier of the sent message.</param>
/// <param name="MatchId">Identifier of the match that originated the conversation.</param>
/// <param name="SenderId">Identifier of the user who sent the message.</param>
/// <param name="RecipientId">Identifier of the user who received the message.</param>
/// <param name="Text">Plain-text content of the message.</param>
/// <param name="OccurredAtUtc">UTC timestamp at which the message was sent.</param>
public sealed record MessagingMessageSentV1(
    Guid ConversationId,
    Guid MessageId,
    Guid MatchId,
    Guid SenderId,
    Guid RecipientId,
    string Text,
    DateTimeOffset OccurredAtUtc);
