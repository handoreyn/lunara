using Lunara.Moderation.Domain.ValueObjects;

namespace Lunara.Moderation.Domain.Entities;

/// <summary>
/// Aggregate root representing a block imposed by one user on another.
/// <para>
/// A block prevents the blocker from appearing in the blocked user's discovery feed,
/// and prevents messaging or matching between the two parties.
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

    /// <summary>Gets the UTC timestamp at which this block was created.</summary>
    public DateTimeOffset CreatedAtUtc { get; }

    private Block(BlockId id, UserId blockerUserId, UserId blockedUserId, DateTimeOffset createdAtUtc)
    {
        Id = id;
        BlockerUserId = blockerUserId;
        BlockedUserId = blockedUserId;
        CreatedAtUtc = createdAtUtc;
    }

    /// <summary>
    /// Creates a new <see cref="Block"/> between two distinct users.
    /// </summary>
    /// <param name="blockerUserId">The user initiating the block.</param>
    /// <param name="blockedUserId">The user being blocked.</param>
    /// <param name="nowUtc">The current UTC timestamp.</param>
    /// <returns>A new <see cref="Block"/> instance.</returns>
    /// <exception cref="ArgumentException">
    ///   Thrown when <paramref name="blockerUserId"/> equals <paramref name="blockedUserId"/>.
    /// </exception>
    public static Block Create(UserId blockerUserId, UserId blockedUserId, DateTimeOffset nowUtc)
    {
        return blockerUserId == blockedUserId
            ? throw new ArgumentException(
                "A user cannot block themselves.",
                nameof(blockedUserId))
            : new Block(new BlockId(Guid.NewGuid()), blockerUserId, blockedUserId, nowUtc);
    }

    /// <summary>
    /// Reconstitutes a <see cref="Block"/> from persisted state.
    /// </summary>
    /// <param name="id">The block identifier.</param>
    /// <param name="blockerUserId">The identifier of the blocking user.</param>
    /// <param name="blockedUserId">The identifier of the blocked user.</param>
    /// <param name="createdAtUtc">The UTC timestamp at which the block was created.</param>
    /// <returns>A reconstituted <see cref="Block"/> instance.</returns>
    public static Block Reconstitute(
        BlockId id,
        UserId blockerUserId,
        UserId blockedUserId,
        DateTimeOffset createdAtUtc)
    {
        return new Block(id, blockerUserId, blockedUserId, createdAtUtc);
    }
}
