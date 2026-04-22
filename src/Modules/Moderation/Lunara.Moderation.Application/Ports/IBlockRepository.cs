using Lunara.Moderation.Domain.Entities;
using Lunara.Moderation.Domain.ValueObjects;

namespace Lunara.Moderation.Application.Ports;

/// <summary>Persistence port for <see cref="Block"/> aggregates.</summary>
public interface IBlockRepository
{
    /// <summary>
    /// Returns <c>true</c> if <paramref name="blockerUserId"/> has already blocked
    /// <paramref name="blockedUserId"/>.
    /// </summary>
    /// <param name="blockerUserId">The user who initiated the block.</param>
    /// <param name="blockedUserId">The user who was blocked.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    Task<bool> ExistsAsync(UserId blockerUserId, UserId blockedUserId, CancellationToken ct);

    /// <summary>
    /// Returns <c>true</c> if a block exists between the two users in either direction.
    /// </summary>
    /// <param name="userA">One of the two users.</param>
    /// <param name="userB">The other user.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    Task<bool> ExistsInEitherDirectionAsync(UserId userA, UserId userB, CancellationToken ct);

    /// <summary>
    /// Adds a new block to the store.
    /// The change is not persisted until <see cref="IUnitOfWork.SaveChangesAsync"/> is called.
    /// </summary>
    /// <param name="block">The block to add.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    Task AddAsync(Block block, CancellationToken ct);
}
