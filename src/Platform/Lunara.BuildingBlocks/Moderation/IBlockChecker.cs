namespace Lunara.BuildingBlocks.Moderation;

/// <summary>
/// Cross-cutting port that allows other modules to check whether a block exists
/// between two users without taking a dependency on the Moderation module.
/// </summary>
public interface IBlockChecker
{
    /// <summary>
    /// Returns <c>true</c> if a block exists between <paramref name="userA"/> and
    /// <paramref name="userB"/> in either direction.
    /// </summary>
    /// <param name="userA">One of the two user identifiers.</param>
    /// <param name="userB">The other user identifier.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    Task<bool> IsBlockedAsync(Guid userA, Guid userB, CancellationToken ct);
}
