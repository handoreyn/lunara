namespace Lunara.Infrastructure.Host;

/// <summary>
/// Checks whether the relational database is reachable and accepting connections.
/// </summary>
public interface IDatabaseReadinessChecker
{
    /// <summary>
    /// Returns <see langword="true"/> if the database is reachable;
    /// <see langword="false"/> if it is not or any non-cancellation error occurs.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    Task<bool> IsReadyAsync(CancellationToken ct);
}
