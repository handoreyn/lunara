using Lunara.BuildingBlocks.Moderation;
using Lunara.Moderation.Application.Ports;
using Lunara.Moderation.Domain.ValueObjects;

namespace Lunara.Infrastructure.Host.Moderation;

/// <summary>
/// Adapts <see cref="IBlockRepository"/> to the <see cref="IBlockChecker"/> cross-cutting interface.
/// This allows the Social and Messaging modules to check for blocks without depending on the
/// Moderation module.
/// </summary>
internal sealed class BlockCheckerAdapter(IBlockRepository blockRepository) : IBlockChecker
{
    /// <inheritdoc/>
    public Task<bool> IsBlockedAsync(Guid userA, Guid userB, CancellationToken ct)
    {
        UserId a = new(userA);
        UserId b = new(userB);
        return blockRepository.ExistsInEitherDirectionAsync(a, b, ct);
    }
}
