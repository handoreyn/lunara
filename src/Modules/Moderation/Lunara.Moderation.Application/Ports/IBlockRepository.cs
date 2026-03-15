using Lunara.Moderation.Domain.ValueObjects;

namespace Lunara.Moderation.Application.Ports;

/// <summary>Persistence port for storing and querying block relationships.</summary>
public interface IBlockRepository
{
    /// <summary>
    /// Returns <c>true</c> if <paramref name="blockerUserId"/> has already blocked <paramref name="blockedUserId"/>.
    /// </summary>
    Task<bool> ExistsAsync(UserId blockerUserId, UserId blockedUserId, CancellationToken ct);

    /// <summary>
    /// Returns <c>true</c> if a block exists in either direction between the two users.
    /// </summary>
    Task<bool> IsBlockedInEitherDirectionAsync(UserId userId1, UserId userId2, CancellationToken ct);

    /// <summary>Persists a new block.</summary>
    Task AddAsync(Domain.Entities.Block block, CancellationToken ct);
}
