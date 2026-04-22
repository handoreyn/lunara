namespace Lunara.Moderation.Application.Ports;

/// <summary>Transactional unit-of-work port for the Moderation module.</summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Commits all pending changes to the underlying store.
    /// </summary>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    Task SaveChangesAsync(CancellationToken ct);
}
