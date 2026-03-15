using Lunara.Infrastructure.Host.Persistence;
using Lunara.Moderation.Application.Ports;

namespace Lunara.Infrastructure.Host.Moderation;

/// <summary>EF Core–backed implementation of <see cref="IUnitOfWork"/> for the Moderation module.</summary>
internal sealed class EfModerationUnitOfWork(LunaraDbContext dbContext) : IUnitOfWork
{
    /// <inheritdoc/>
    public Task SaveChangesAsync(CancellationToken ct)
    {
        return dbContext.SaveChangesAsync(ct);
    }
}
