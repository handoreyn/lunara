using Lunara.BuildingBlocks.Clocks;
using Lunara.Moderation.Application.DTOs;
using Lunara.Moderation.Application.Ports;
using Lunara.Moderation.Domain.Entities;

namespace Lunara.Moderation.Application.UseCases;

/// <summary>
/// Handles blocking one user by another.
/// Duplicate blocks are silently treated as a no-op.
/// </summary>
public sealed class BlockUserService(
    IBlockRepository blockRepository,
    IUnitOfWork unitOfWork,
    IClock clock)
{
    /// <summary>
    /// Blocks <paramref name="request"/>.<c>BlockedUserId</c> on behalf of
    /// <paramref name="request"/>.<c>BlockerUserId</c>.
    /// If the block already exists the operation is a no-op.
    /// </summary>
    /// <param name="request">The block details.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A <see cref="BlockUserResult"/> describing the outcome.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when blocker and blocked are the same user.</exception>
    public async Task<BlockUserResult> BlockAsync(
        BlockUserRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Domain validation: same-user guard is enforced by Block.Create, but we let it propagate.
        bool alreadyBlocked = await blockRepository.ExistsAsync(
            request.BlockerUserId, request.BlockedUserId, ct)
            .ConfigureAwait(false);

        if (alreadyBlocked)
        {
            return new BlockUserResult(AlreadyBlocked: true, BlockId: null);
        }

        Block block = Block.Create(request.BlockerUserId, request.BlockedUserId, clock.UtcNow);

        await blockRepository.AddAsync(block, ct).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return new BlockUserResult(AlreadyBlocked: false, BlockId: block.Id.Value);
    }
}
