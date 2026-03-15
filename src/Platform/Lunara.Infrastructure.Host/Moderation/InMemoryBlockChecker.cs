using Lunara.BuildingBlocks.Moderation;
using Lunara.Moderation.Application.Ports;
using Lunara.Moderation.Domain.ValueObjects;

namespace Lunara.Infrastructure.Host.Moderation;

/// <summary>
/// In-memory implementation of <see cref="IBlockChecker"/> that delegates to
/// the in-memory <see cref="IBlockRepository"/>.
/// </summary>
internal sealed class InMemoryBlockChecker(IBlockRepository blockRepository) : IBlockChecker
{
    /// <inheritdoc/>
    public Task<bool> IsBlockedInEitherDirectionAsync(Guid userId1, Guid userId2, CancellationToken ct)
    {
        UserId id1 = new(userId1);
        UserId id2 = new(userId2);
        return blockRepository.IsBlockedInEitherDirectionAsync(id1, id2, ct);
    }
}
