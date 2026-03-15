using Lunara.Messaging.Domain.ValueObjects;

namespace Lunara.Messaging.Domain.Entities;

/// <summary>
/// Represents a single message sent within a <see cref="Conversation"/>.
/// </summary>
public sealed class Message
{
    /// <summary>Gets the unique identifier of this message.</summary>
    public MessageId Id { get; }

    /// <summary>Gets the identifier of the conversation this message belongs to.</summary>
    public ConversationId ConversationId { get; }

    /// <summary>Gets the identifier of the user who sent this message.</summary>
    public UserId SenderId { get; }

    /// <summary>Gets the identifier of the user who receives this message.</summary>
    public UserId RecipientId { get; }

    /// <summary>Gets the trimmed text content of the message.</summary>
    public string Text { get; }

    /// <summary>Gets the UTC timestamp at which this message was sent.</summary>
    public DateTimeOffset SentAtUtc { get; }

    internal Message(
        MessageId id,
        ConversationId conversationId,
        UserId senderId,
        UserId recipientId,
        string text,
        DateTimeOffset sentAtUtc)
    {
        Id = id;
        ConversationId = conversationId;
        SenderId = senderId;
        RecipientId = recipientId;
        Text = text;
        SentAtUtc = sentAtUtc;
    }
}
