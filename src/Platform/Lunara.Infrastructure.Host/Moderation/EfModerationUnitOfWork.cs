using Lunara.Infrastructure.Host.Persistence;
using Lunara.Moderation.Application.Exceptions;
using Lunara.Moderation.Application.Ports;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Lunara.Infrastructure.Host.Moderation;

/// <summary>EF Core–backed implementation of <see cref="IUnitOfWork"/> for the Moderation module.</summary>
internal sealed class EfModerationUnitOfWork(LunaraDbContext dbContext) : IUnitOfWork
{
    // PostgreSQL error code for unique_violation (https://www.postgresql.org/docs/current/errcodes-appendix.html)
    private const string UniqueViolationSqlState = "23505";

    /// <inheritdoc/>
    /// <exception cref="DuplicateEntityException">
    ///   Thrown when saving detects a unique-constraint violation, allowing callers to treat
    ///   the operation as an idempotent no-op.
    /// </exception>
    public async Task SaveChangesAsync(CancellationToken ct)
    {
        try
        {
            await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException pgEx
               && pgEx.SqlState == UniqueViolationSqlState)
        {
            throw new DuplicateEntityException(
                "A duplicate record was detected; the operation is a no-op.",
                ex);
        }
    }
}
