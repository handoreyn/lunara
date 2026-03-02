namespace Lunara.Messaging.Infrastructure.Persistence;

/// <summary>
/// EF Core persistence row for a <see cref="Domain.Entities.Message"/> entity.
/// All columns are raw <see cref="Guid"/> / primitive values; no domain types cross the persistence boundary.
/// </summary>
public sealed class MessageEntity
{
    /// <summary>Gets or sets the unique identifier of the message.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the identifier of the conversation this message belongs to.</summary>
    public Guid ConversationId { get; set; }

    /// <summary>Gets or sets the identifier of the user who sent this message.</summary>
    public Guid SenderId { get; set; }

    /// <summary>Gets or sets the identifier of the user who received this message.</summary>
    public Guid RecipientId { get; set; }

    /// <summary>Gets or sets the trimmed text content of the message.</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>Gets or sets the UTC timestamp at which this message was sent.</summary>
    public DateTimeOffset SentAtUtc { get; set; }

    /// <summary>Gets or sets the navigation property to the owning conversation.</summary>
    public ConversationEntity? Conversation { get; set; }
}
