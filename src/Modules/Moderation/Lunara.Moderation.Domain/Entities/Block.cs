using Lunara.Moderation.Domain.ValueObjects;

namespace Lunara.Moderation.Domain.Entities;

/// <summary>
/// Represents a block relationship where one user has blocked another.
/// A user cannot block themselves.
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
    /// Creates a new <see cref="Block"/> between two users.
    /// </summary>
    /// <param name="blockerUserId">The user initiating the block.</param>
    /// <param name="blockedUserId">The user being blocked.</param>
    /// <param name="createdAtUtc">The UTC timestamp of the block action.</param>
    /// <returns>A new <see cref="Block"/> instance.</returns>
    /// <exception cref="ArgumentException">
    ///   Thrown when <paramref name="blockerUserId"/> equals <paramref name="blockedUserId"/>.
    /// </exception>
    public static Block Create(
        UserId blockerUserId,
        UserId blockedUserId,
        DateTimeOffset createdAtUtc)
    {
        if (blockerUserId == blockedUserId)
        {
            throw new ArgumentException(
                "A user cannot block themselves.",
                nameof(blockedUserId));
        }

        BlockId id = new(Guid.NewGuid());
        return new Block(id, blockerUserId, blockedUserId, createdAtUtc);
    }
}
