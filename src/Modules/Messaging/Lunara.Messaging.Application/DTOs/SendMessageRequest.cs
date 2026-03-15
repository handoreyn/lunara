using Lunara.Messaging.Domain.ValueObjects;

namespace Lunara.Messaging.Application.DTOs;

/// <summary>Input data for the send-message use case.</summary>
/// <param name="SenderId">The user sending the message.</param>
/// <param name="RecipientId">The user receiving the message.</param>
/// <param name="Body">The message body text.</param>
public sealed record SendMessageRequest(UserId SenderId, UserId RecipientId, string Body);
