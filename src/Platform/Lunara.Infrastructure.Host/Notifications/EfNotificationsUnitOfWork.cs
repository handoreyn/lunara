using Lunara.Infrastructure.Host.Persistence;
using Lunara.Notifications.Application.Ports;
using Microsoft.EntityFrameworkCore;

namespace Lunara.Infrastructure.Host.Notifications;

/// <summary>
/// EF Core implementation of <see cref="IUnitOfWork"/> for the Notifications module.
/// Delegates to the shared <see cref="LunaraDbContext"/> to persist all pending changes.
/// On failure the change tracker is cleared so that retry attempts start from a clean state.
/// </summary>
internal sealed class EfNotificationsUnitOfWork(LunaraDbContext dbContext) : IUnitOfWork
{
    /// <inheritdoc/>
    public async Task SaveChangesAsync(CancellationToken ct)
    {
        try
        {
            await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
        }
        catch (DbUpdateException)
        {
            dbContext.ChangeTracker.Clear();
            throw;
        }
    }
}
