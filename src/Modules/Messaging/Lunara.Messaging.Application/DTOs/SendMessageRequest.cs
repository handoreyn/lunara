using Lunara.Messaging.Domain.ValueObjects;

namespace Lunara.Messaging.Application.DTOs;

/// <summary>Input for the <see cref="UseCases.SendMessageService.SendAsync"/> use-case.</summary>
/// <param name="SenderId">The user sending the message.</param>
/// <param name="RecipientId">The user receiving the message.</param>
/// <param name="Content">The message text content.</param>
public sealed record SendMessageRequest(UserId SenderId, UserId RecipientId, string Content);
