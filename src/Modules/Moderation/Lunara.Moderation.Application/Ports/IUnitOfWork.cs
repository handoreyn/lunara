namespace Lunara.Moderation.Application.Ports;

/// <summary>Persistence port for persisting <see cref="Domain.Entities.Block"/> entities.</summary>
public interface IUnitOfWork
{
    /// <summary>Persists all pending changes to the underlying store.</summary>
    Task SaveChangesAsync(CancellationToken ct);
}
