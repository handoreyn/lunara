using Lunara.Messaging.Domain.ValueObjects;

namespace Lunara.Messaging.Domain.Entities;

/// <summary>
/// Represents a message sent from one user to another.
/// A user cannot send a message to themselves.
/// </summary>
public sealed class Message
{
    /// <summary>Maximum allowed length for the <see cref="Body"/> field.</summary>
    public const int MaxBodyLength = 4000;

    /// <summary>Gets the unique identifier of this message.</summary>
    public MessageId Id { get; }

    /// <summary>Gets the identifier of the user who sent the message.</summary>
    public UserId SenderId { get; }

    /// <summary>Gets the identifier of the user who receives the message.</summary>
    public UserId RecipientId { get; }

    /// <summary>Gets the message body text.</summary>
    public string Body { get; }

    /// <summary>Gets the UTC timestamp at which the message was created.</summary>
    public DateTimeOffset CreatedAtUtc { get; }

    private Message(
        MessageId id,
        UserId senderId,
        UserId recipientId,
        string body,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        SenderId = senderId;
        RecipientId = recipientId;
        Body = body;
        CreatedAtUtc = createdAtUtc;
    }

    /// <summary>
    /// Creates a new <see cref="Message"/> from <paramref name="senderId"/> to <paramref name="recipientId"/>.
    /// </summary>
    /// <param name="senderId">The user sending the message.</param>
    /// <param name="recipientId">The user receiving the message.</param>
    /// <param name="body">The message body. Must not exceed <see cref="MaxBodyLength"/> characters.</param>
    /// <param name="createdAtUtc">The UTC timestamp of the send action.</param>
    /// <returns>A new <see cref="Message"/> instance.</returns>
    /// <exception cref="ArgumentException">
    ///   Thrown when <paramref name="senderId"/> equals <paramref name="recipientId"/>,
    ///   or when <paramref name="body"/> is null, empty, or too long.
    /// </exception>
    public static Message Create(
        UserId senderId,
        UserId recipientId,
        string body,
        DateTimeOffset createdAtUtc)
    {
        if (senderId == recipientId)
        {
            throw new ArgumentException(
                "A user cannot send a message to themselves.",
                nameof(recipientId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(body, nameof(body));

        if (body.Length > MaxBodyLength)
        {
            throw new ArgumentException(
                $"Message body must not exceed {MaxBodyLength} characters.",
                nameof(body));
        }

        MessageId id = new(Guid.NewGuid());
        return new Message(id, senderId, recipientId, body, createdAtUtc);
    }
}
