namespace Lunara.Social.Application.Ports;

/// <summary>Commits all pending changes to the underlying data store.</summary>
public interface IUnitOfWork
{
    /// <summary>Persists all pending changes atomically.</summary>
    Task SaveChangesAsync(CancellationToken ct);
}
