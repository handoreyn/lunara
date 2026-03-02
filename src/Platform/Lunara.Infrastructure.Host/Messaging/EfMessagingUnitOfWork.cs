using Lunara.Messaging.Application.Ports;
using Lunara.Infrastructure.Host.Persistence;

namespace Lunara.Infrastructure.Host.Messaging;

/// <summary>
/// EF Core implementation of <see cref="IUnitOfWork"/> for the Messaging module.
/// Delegates to the shared <see cref="LunaraDbContext"/> to persist all pending changes.
/// On failure the change tracker is cleared so that retry attempts start from a clean state.
/// </summary>
internal sealed class EfMessagingUnitOfWork(LunaraDbContext dbContext) : IUnitOfWork
{
    /// <inheritdoc/>
    public async Task SaveChangesAsync(CancellationToken ct)
    {
        try
        {
            await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
        }
        catch
        {
            dbContext.ChangeTracker.Clear();
            throw;
        }
    }
}
