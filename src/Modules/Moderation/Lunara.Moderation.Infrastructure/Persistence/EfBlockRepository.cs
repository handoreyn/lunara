using Lunara.Moderation.Application.Ports;
using Lunara.Moderation.Domain.Entities;
using Lunara.Moderation.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Lunara.Moderation.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of <see cref="IBlockRepository"/>.
/// </summary>
internal sealed class EfBlockRepository(DbContext context) : IBlockRepository
{
    private DbSet<BlockEntity> Blocks => context.Set<BlockEntity>();

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(
        UserId blockerUserId,
        UserId blockedUserId,
        CancellationToken ct)
    {
        return await Blocks.AnyAsync(
            b => b.BlockerUserId == blockerUserId.Value
              && b.BlockedUserId == blockedUserId.Value,
            ct)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> IsBlockedInEitherDirectionAsync(
        UserId userId1,
        UserId userId2,
        CancellationToken ct)
    {
        return await Blocks.AnyAsync(
            b => (b.BlockerUserId == userId1.Value && b.BlockedUserId == userId2.Value)
              || (b.BlockerUserId == userId2.Value && b.BlockedUserId == userId1.Value),
            ct)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task AddAsync(Block block, CancellationToken ct)
    {
        BlockEntity entity = new()
        {
            Id = block.Id.Value,
            BlockerUserId = block.BlockerUserId.Value,
            BlockedUserId = block.BlockedUserId.Value,
            CreatedAtUtc = block.CreatedAtUtc,
        };

        await Blocks.AddAsync(entity, ct).ConfigureAwait(false);
    }
}
