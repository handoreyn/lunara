namespace Lunara.Messaging.Application.DTOs;

/// <summary>Output of the <see cref="UseCases.SendMessageService.SendAsync"/> use-case.</summary>
/// <param name="Sent"><c>true</c> when the message was accepted for delivery.</param>
/// <param name="Blocked"><c>true</c> when the message was rejected because a block exists in either direction.</param>
public sealed record SendMessageResult(bool Sent, bool Blocked);
