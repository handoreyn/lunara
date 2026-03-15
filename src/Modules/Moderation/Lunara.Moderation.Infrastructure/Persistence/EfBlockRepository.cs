using Lunara.Moderation.Application.Ports;
using Lunara.Moderation.Domain.Entities;
using Lunara.Moderation.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Lunara.Moderation.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of <see cref="IBlockRepository"/>.
/// </summary>
internal sealed class EfBlockRepository(DbContext dbContext) : IBlockRepository
{
    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(UserId blockerUserId, UserId blockedUserId, CancellationToken ct)
    {
        return await dbContext.Set<BlockEntity>()
            .AnyAsync(
                b => b.BlockerUserId == blockerUserId.Value && b.BlockedUserId == blockedUserId.Value,
                ct)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsInEitherDirectionAsync(UserId userA, UserId userB, CancellationToken ct)
    {
        return await dbContext.Set<BlockEntity>()
            .AnyAsync(
                b => (b.BlockerUserId == userA.Value && b.BlockedUserId == userB.Value)
                  || (b.BlockerUserId == userB.Value && b.BlockedUserId == userA.Value),
                ct)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task AddAsync(Block block, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(block);

        BlockEntity entity = ToEntity(block);
        await dbContext.Set<BlockEntity>().AddAsync(entity, ct).ConfigureAwait(false);
    }

    // ── Mapping ────────────────────────────────────────────────────────────

    private static BlockEntity ToEntity(Block b)
    {
        return new BlockEntity
        {
            Id = b.Id.Value,
            BlockerUserId = b.BlockerUserId.Value,
            BlockedUserId = b.BlockedUserId.Value,
            CreatedAtUtc = b.CreatedAtUtc,
        };
    }
}
