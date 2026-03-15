using Lunara.Moderation.Domain.Entities;
using Lunara.Moderation.Domain.ValueObjects;

namespace Lunara.Moderation.Application.Ports;

/// <summary>Persistence port for querying and storing <see cref="Block"/> entities.</summary>
public interface IBlockRepository
{
    /// <summary>
    /// Returns <c>true</c> when <paramref name="blockerUserId"/> has already blocked <paramref name="blockedUserId"/>.
    /// </summary>
    Task<bool> ExistsAsync(UserId blockerUserId, UserId blockedUserId, CancellationToken ct);

    /// <summary>Persists a new <paramref name="block"/>.</summary>
    Task AddAsync(Block block, CancellationToken ct);

    /// <summary>
    /// Returns <c>true</c> when either
    /// <paramref name="userId1"/> has blocked <paramref name="userId2"/> or
    /// <paramref name="userId2"/> has blocked <paramref name="userId1"/>.
    /// </summary>
    Task<bool> IsBlockedInEitherDirectionAsync(UserId userId1, UserId userId2, CancellationToken ct);
}
