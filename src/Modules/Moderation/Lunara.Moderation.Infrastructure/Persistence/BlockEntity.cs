namespace Lunara.Moderation.Infrastructure.Persistence;

/// <summary>EF Core entity representing a row in the <c>moderation_blocks</c> table.</summary>
internal sealed class BlockEntity
{
    /// <summary>Gets or sets the primary key.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the identifier of the user who initiated the block.</summary>
    public Guid BlockerUserId { get; set; }

    /// <summary>Gets or sets the identifier of the user who was blocked.</summary>
    public Guid BlockedUserId { get; set; }

    /// <summary>Gets or sets the UTC timestamp at which the block was created.</summary>
    public DateTimeOffset CreatedAtUtc { get; set; }
}
