using Lunara.Messaging.Domain.ValueObjects;

namespace Lunara.Messaging.Domain.DomainEvents;

/// <summary>
/// Raised when a <see cref="Entities.Message"/> is successfully sent within a
/// <see cref="Entities.Conversation"/>.
/// </summary>
/// <param name="EventId">A unique identifier for this event occurrence.</param>
/// <param name="ConversationId">The conversation in which the message was sent.</param>
/// <param name="MessageId">The unique identifier of the new message.</param>
/// <param name="SenderId">The user who sent the message.</param>
/// <param name="RecipientId">The user who received the message.</param>
/// <param name="Text">The trimmed text content of the message.</param>
/// <param name="OccurredAtUtc">The UTC timestamp at which the message was sent.</param>
public sealed record MessageSent(
    Guid EventId,
    ConversationId ConversationId,
    MessageId MessageId,
    UserId SenderId,
    UserId RecipientId,
    string Text,
    DateTimeOffset OccurredAtUtc);
