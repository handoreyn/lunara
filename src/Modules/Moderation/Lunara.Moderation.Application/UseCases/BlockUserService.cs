using Lunara.BuildingBlocks.Clocks;
using Lunara.Moderation.Application.DTOs;
using Lunara.Moderation.Application.Exceptions;
using Lunara.Moderation.Application.Ports;
using Lunara.Moderation.Domain.Entities;

namespace Lunara.Moderation.Application.UseCases;

/// <summary>
/// Records a block from one user to another.
/// <para>
/// Duplicate blocks (blocker/blocked pair already exists) are silently treated as a no-op.
/// </para>
/// </summary>
public sealed class BlockUserService(
    IBlockRepository blockRepository,
    IUnitOfWork unitOfWork,
    IClock clock)
{
    /// <summary>
    /// Processes a block request and returns the outcome.
    /// </summary>
    /// <param name="request">The block details.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>
    ///   A <see cref="BlockUserResult"/> indicating whether a new block was created.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    ///   Thrown when <paramref name="request"/> has equal <c>BlockerUserId</c> and <c>BlockedUserId</c>.
    /// </exception>
    public async Task<BlockUserResult> BlockAsync(BlockUserRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.BlockerUserId == request.BlockedUserId)
        {
            throw new ArgumentException(
                "A user cannot block themselves.",
                nameof(request));
        }

        // Fast path: avoid a DB write for the common case where a block already exists.
        bool alreadyBlocked = await blockRepository.ExistsAsync(
            request.BlockerUserId, request.BlockedUserId, ct)
            .ConfigureAwait(false);

        if (alreadyBlocked)
        {
            return new BlockUserResult(BlockCreated: false);
        }

        Block block = Block.Create(request.BlockerUserId, request.BlockedUserId, clock.UtcNow);
        await blockRepository.AddAsync(block, ct).ConfigureAwait(false);

        try
        {
            await unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);
        }
        catch (DuplicateEntityException)
        {
            // A concurrent request inserted the same block between our ExistsAsync check
            // and this SaveChangesAsync.  Treat as no-op — the block already exists.
            return new BlockUserResult(BlockCreated: false);
        }

        return new BlockUserResult(BlockCreated: true);
    }
}
