namespace Lunara.Moderation.Infrastructure.Persistence;

/// <summary>
/// EF Core persistence row for a <see cref="Domain.Entities.Block"/> aggregate.
/// All columns are raw primitives; no domain value objects cross the persistence boundary.
/// </summary>
public sealed class BlockEntity
{
    /// <summary>Gets or sets the unique identifier of the block.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the identifier of the user who initiated the block.</summary>
    public Guid BlockerUserId { get; set; }

    /// <summary>Gets or sets the identifier of the user who was blocked.</summary>
    public Guid BlockedUserId { get; set; }

    /// <summary>Gets or sets the UTC timestamp at which this block was created.</summary>
    public DateTimeOffset CreatedAtUtc { get; set; }
}
