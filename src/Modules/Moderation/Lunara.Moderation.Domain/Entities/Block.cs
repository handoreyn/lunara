using Lunara.Moderation.Domain.ValueObjects;

namespace Lunara.Moderation.Domain.Entities;

/// <summary>
/// Aggregate root representing one user blocking another.
/// <para>
/// A user cannot block themselves.
/// </para>
/// </summary>
public sealed class Block
{
    /// <summary>Gets the unique identifier of this block.</summary>
    public BlockId Id { get; }

    /// <summary>Gets the identifier of the user who initiated the block.</summary>
    public UserId BlockerUserId { get; }

    /// <summary>Gets the identifier of the user who was blocked.</summary>
    public UserId BlockedUserId { get; }

    /// <summary>Gets the UTC timestamp at which the block was created.</summary>
    public DateTimeOffset CreatedAtUtc { get; }

    private Block(BlockId id, UserId blockerUserId, UserId blockedUserId, DateTimeOffset createdAtUtc)
    {
        Id = id;
        BlockerUserId = blockerUserId;
        BlockedUserId = blockedUserId;
        CreatedAtUtc = createdAtUtc;
    }

    /// <summary>
    /// Creates a new <see cref="Block"/>.
    /// </summary>
    /// <param name="blockerUserId">The user initiating the block.</param>
    /// <param name="blockedUserId">The user being blocked.</param>
    /// <param name="createdAtUtc">The UTC timestamp of the block.</param>
    /// <returns>A new <see cref="Block"/> instance.</returns>
    /// <exception cref="ArgumentException">
    ///   Thrown when <paramref name="blockerUserId"/> equals <paramref name="blockedUserId"/>.
    /// </exception>
    public static Block Create(UserId blockerUserId, UserId blockedUserId, DateTimeOffset createdAtUtc)
    {
        return blockerUserId != blockedUserId
            ? new Block(new BlockId(Guid.NewGuid()), blockerUserId, blockedUserId, createdAtUtc)
            : throw new ArgumentException(
                "A user cannot block themselves.",
                nameof(blockedUserId));
    }
}
