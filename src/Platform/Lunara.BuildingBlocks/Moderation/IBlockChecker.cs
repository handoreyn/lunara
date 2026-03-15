namespace Lunara.BuildingBlocks.Moderation;

/// <summary>
/// Cross-cutting port for checking whether a block exists between two users.
/// <para>
/// Consumers (Social, Messaging) call this before performing actions that must be
/// blocked when either party has blocked the other.
/// Uses raw <see cref="Guid"/> values to avoid coupling to any module's domain types.
/// </para>
/// </summary>
public interface IBlockChecker
{
    /// <summary>
    /// Returns <c>true</c> when a block exists in either direction between
    /// <paramref name="userId1"/> and <paramref name="userId2"/>.
    /// </summary>
    /// <param name="userId1">The first user identifier.</param>
    /// <param name="userId2">The second user identifier.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    Task<bool> IsBlockedInEitherDirectionAsync(Guid userId1, Guid userId2, CancellationToken ct);
}
