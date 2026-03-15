namespace Lunara.Messaging.Application.DTOs;

/// <summary>Output of the <see cref="UseCases.SendMessageService.SendAsync"/> use-case.</summary>
/// <param name="ConversationId">The identifier of the conversation the message was added to.</param>
/// <param name="MessageId">The identifier of the newly created message.</param>
public sealed record SendMessageResult(
    Guid ConversationId,
    Guid MessageId);
