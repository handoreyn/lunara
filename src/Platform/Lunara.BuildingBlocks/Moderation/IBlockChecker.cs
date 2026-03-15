namespace Lunara.BuildingBlocks.Moderation;

/// <summary>
/// Cross-cutting port used by modules (Social, Messaging, etc.) to check whether
/// a block relationship exists between two users in either direction.
/// </summary>
public interface IBlockChecker
{
    /// <summary>
    /// Returns <c>true</c> when a block exists between <paramref name="userId1"/> and
    /// <paramref name="userId2"/> in either direction (A blocked B or B blocked A).
    /// </summary>
    /// <param name="userId1">First user identifier.</param>
    /// <param name="userId2">Second user identifier.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    Task<bool> IsBlockedInEitherDirectionAsync(Guid userId1, Guid userId2, CancellationToken ct);
}
