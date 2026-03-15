namespace Lunara.Moderation.Application.Ports;

/// <summary>Persistence boundary for the Moderation module.</summary>
public interface IUnitOfWork
{
    /// <summary>Persists all pending changes to the underlying store.</summary>
    Task SaveChangesAsync(CancellationToken ct);
}
