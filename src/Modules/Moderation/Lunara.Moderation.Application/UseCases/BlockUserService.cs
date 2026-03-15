using Lunara.BuildingBlocks.Clocks;
using Lunara.Moderation.Application.DTOs;
using Lunara.Moderation.Application.Ports;
using Lunara.Moderation.Domain.Entities;
using Lunara.Moderation.Domain.ValueObjects;

namespace Lunara.Moderation.Application.UseCases;

/// <summary>
/// Blocks a user on behalf of another user.
/// Duplicate block requests are treated as a no-op.
/// </summary>
public sealed class BlockUserService(
    IBlockRepository blockRepository,
    IUnitOfWork unitOfWork,
    IClock clock)
{
    /// <summary>
    /// Blocks <see cref="BlockUserRequest.BlockedUserId"/> on behalf of
    /// <see cref="BlockUserRequest.BlockerUserId"/>.
    /// If the block already exists, returns immediately without creating a duplicate.
    /// </summary>
    /// <param name="request">The block request.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>
    ///   A <see cref="BlockUserResult"/> indicating whether a new block was created or a
    ///   duplicate was detected.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   Thrown when <paramref name="request"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   Thrown when <see cref="BlockUserRequest.BlockerUserId"/> equals
    ///   <see cref="BlockUserRequest.BlockedUserId"/>.
    /// </exception>
    public async Task<BlockUserResult> BlockAsync(BlockUserRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        UserId blockerUserId = new(request.BlockerUserId);
        UserId blockedUserId = new(request.BlockedUserId);

        bool alreadyBlocked = await blockRepository
            .ExistsAsync(blockerUserId, blockedUserId, ct)
            .ConfigureAwait(false);

        if (alreadyBlocked)
        {
            return new BlockUserResult(BlockId: null, AlreadyBlocked: true);
        }

        Block block = Block.Create(blockerUserId, blockedUserId, clock.UtcNow);

        await blockRepository.AddAsync(block, ct).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return new BlockUserResult(BlockId: block.Id.Value, AlreadyBlocked: false);
    }
}
