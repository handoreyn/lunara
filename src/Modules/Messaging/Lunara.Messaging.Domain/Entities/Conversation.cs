using Lunara.Messaging.Domain.DomainEvents;
using Lunara.Messaging.Domain.ValueObjects;

namespace Lunara.Messaging.Domain.Entities;

/// <summary>
/// Aggregate root representing a messaging conversation between two matched users.
/// <para>
/// The participant pair is stored in canonical order: <see cref="User1Id"/> always holds the
/// lexicographically smaller <see cref="Guid"/>, <see cref="User2Id"/> the larger.
/// This guarantees that a given pair maps to exactly one <see cref="Conversation"/> record.
/// </para>
/// </summary>
public sealed class Conversation
{
    private readonly List<Message> _messages = [];

    /// <summary>Gets the unique identifier of this conversation.</summary>
    public ConversationId Id { get; }

    /// <summary>Gets the social match that originated this conversation.</summary>
    public MatchId MatchId { get; }

    /// <summary>Gets the participant with the lexicographically smaller user identifier.</summary>
    public UserId User1Id { get; }

    /// <summary>Gets the participant with the lexicographically larger user identifier.</summary>
    public UserId User2Id { get; }

    /// <summary>Gets the UTC timestamp at which this conversation was created.</summary>
    public DateTimeOffset CreatedAtUtc { get; }

    /// <summary>Gets the ordered list of messages in this conversation.</summary>
    public IReadOnlyList<Message> Messages => _messages;

    private Conversation(
        ConversationId id,
        MatchId matchId,
        UserId user1Id,
        UserId user2Id,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        MatchId = matchId;
        User1Id = user1Id;
        User2Id = user2Id;
        CreatedAtUtc = createdAtUtc;
    }

    /// <summary>
    /// Creates a new <see cref="Conversation"/> from the given match, enforcing canonical
    /// participant ordering (smaller <see cref="Guid"/> → <see cref="User1Id"/>).
    /// </summary>
    /// <param name="matchId">The originating match identifier.</param>
    /// <param name="user1">One of the two participants.</param>
    /// <param name="user2">The other participant.</param>
    /// <param name="nowUtc">The current UTC time.</param>
    /// <returns>A new <see cref="Conversation"/> instance.</returns>
    /// <exception cref="ArgumentException">
    ///   Thrown when <paramref name="user1"/> and <paramref name="user2"/> have the same identifier.
    /// </exception>
    public static Conversation CreateFromMatch(
        MatchId matchId,
        UserId user1,
        UserId user2,
        DateTimeOffset nowUtc)
    {
        if (user1 == user2)
        {
            throw new ArgumentException("A conversation must involve two distinct users.", nameof(user2));
        }

        (UserId smaller, UserId larger) = user1.Value.CompareTo(user2.Value) < 0
            ? (user1, user2)
            : (user2, user1);

        return new Conversation(
            id: new ConversationId(Guid.NewGuid()),
            matchId: matchId,
            user1Id: smaller,
            user2Id: larger,
            createdAtUtc: nowUtc);
    }

    /// <summary>
    /// Reconstitutes a <see cref="Conversation"/> from a previously persisted state.
    /// Canonical ordering is assumed to have been enforced at creation time;
    /// no reordering is applied.
    /// </summary>
    /// <param name="id">The conversation identifier.</param>
    /// <param name="matchId">The originating match identifier.</param>
    /// <param name="user1Id">The participant with the lexicographically smaller identifier.</param>
    /// <param name="user2Id">The participant with the lexicographically larger identifier.</param>
    /// <param name="createdAtUtc">The UTC timestamp at which the conversation was originally created.</param>
    /// <returns>A reconstituted <see cref="Conversation"/> instance with an empty messages list.</returns>
    public static Conversation Reconstitute(
        ConversationId id,
        MatchId matchId,
        UserId user1Id,
        UserId user2Id,
        DateTimeOffset createdAtUtc)
    {
        return new Conversation(id, matchId, user1Id, user2Id, createdAtUtc);
    }

    /// <summary>
    /// Appends a new <see cref="Message"/> to this conversation and returns both the message
    /// and the corresponding <see cref="MessageSent"/> domain event.
    /// </summary>
    /// <param name="senderId">The user sending the message. Must be a participant of this conversation.</param>
    /// <param name="text">The message text. Will be trimmed; must be between 1 and 2000 characters.</param>
    /// <param name="nowUtc">The current UTC time.</param>
    /// <returns>
    ///   A tuple of the newly created <see cref="Message"/> and the <see cref="MessageSent"/> event.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///   Thrown when <paramref name="senderId"/> is not a participant, or when the trimmed
    ///   <paramref name="text"/> is empty or exceeds 2000 characters.
    /// </exception>
    public (Message Message, MessageSent Event) SendMessage(UserId senderId, string text, DateTimeOffset nowUtc)
    {
        if (senderId != User1Id && senderId != User2Id)
        {
            throw new ArgumentException(
                "Sender is not a participant of this conversation.",
                nameof(senderId));
        }

        string trimmedText = text?.Trim() ?? string.Empty;

        if (trimmedText.Length == 0)
        {
            throw new ArgumentException("Message text must not be empty.", nameof(text));
        }

        if (trimmedText.Length > 2000)
        {
            throw new ArgumentException(
                "Message text must not exceed 2000 characters.",
                nameof(text));
        }

        UserId recipientId = senderId == User1Id ? User2Id : User1Id;

        Message message = new(
            id: new MessageId(Guid.NewGuid()),
            conversationId: Id,
            senderId: senderId,
            recipientId: recipientId,
            text: trimmedText,
            sentAtUtc: nowUtc);

        _messages.Add(message);

        MessageSent evt = new(
            EventId: Guid.NewGuid(),
            ConversationId: Id,
            MessageId: message.Id,
            SenderId: senderId,
            RecipientId: recipientId,
            Text: trimmedText,
            OccurredAtUtc: nowUtc);

        return (message, evt);
    }
}
