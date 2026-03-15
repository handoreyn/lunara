namespace Lunara.Messaging.Infrastructure.Persistence;

/// <summary>
/// EF Core persistence row for a <see cref="Domain.Entities.Conversation"/> aggregate.
/// All columns are raw <see cref="Guid"/> / primitive values; no domain types cross the persistence boundary.
/// </summary>
public sealed class ConversationEntity
{
    /// <summary>Gets or sets the unique identifier of the conversation.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the identifier of the social match that originated this conversation.</summary>
    public Guid MatchId { get; set; }

    /// <summary>Gets or sets the participant with the lexicographically smaller user identifier.</summary>
    public Guid User1Id { get; set; }

    /// <summary>Gets or sets the participant with the lexicographically larger user identifier.</summary>
    public Guid User2Id { get; set; }

    /// <summary>Gets or sets the UTC timestamp at which this conversation was created.</summary>
    public DateTimeOffset CreatedAtUtc { get; set; }
}
