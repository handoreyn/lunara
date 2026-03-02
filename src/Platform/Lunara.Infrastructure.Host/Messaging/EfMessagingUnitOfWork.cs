using Lunara.Messaging.Application.Ports;
using Lunara.Infrastructure.Host.Persistence;

namespace Lunara.Infrastructure.Host.Messaging;

/// <summary>
/// EF Core implementation of <see cref="IUnitOfWork"/> for the Messaging module.
/// Delegates to the shared <see cref="LunaraDbContext"/> to persist all pending changes.
/// </summary>
internal sealed class EfMessagingUnitOfWork(LunaraDbContext dbContext) : IUnitOfWork
{
    /// <inheritdoc/>
    public Task SaveChangesAsync(CancellationToken ct)
    {
        return dbContext.SaveChangesAsync(ct);
    }
}
