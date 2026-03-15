namespace Lunara.Messaging.Application.DTOs;

/// <summary>Result of the send-message use case.</summary>
/// <param name="MessageId">The identifier of the newly created message.</param>
public sealed record SendMessageResult(Guid MessageId);
