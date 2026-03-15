using Lunara.BuildingBlocks.Moderation;
using Lunara.Moderation.Application.Ports;
using Lunara.Moderation.Domain.ValueObjects;

namespace Lunara.Infrastructure.Host.Moderation;

/// <summary>
/// Adapts <see cref="IBlockRepository"/> to satisfy the cross-cutting <see cref="IBlockChecker"/>
/// interface consumed by Social, Messaging, and other modules.
/// </summary>
internal sealed class BlockCheckerAdapter(IBlockRepository blockRepository) : IBlockChecker
{
    /// <inheritdoc/>
    public Task<bool> IsBlockedInEitherDirectionAsync(
        Guid userId1,
        Guid userId2,
        CancellationToken ct)
    {
        return blockRepository.IsBlockedInEitherDirectionAsync(
            new UserId(userId1),
            new UserId(userId2),
            ct);
    }
}
