using Lunara.Infrastructure.Host.Persistence;
using Lunara.Messaging.Application.Exceptions;
using Lunara.Messaging.Application.Ports;
using Lunara.Messaging.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using MessagingMatchId = Lunara.Messaging.Domain.ValueObjects.MatchId;

namespace Lunara.Infrastructure.Host.Messaging;

/// <summary>
/// EF Core implementation of <see cref="IUnitOfWork"/> for the Messaging module.
/// Delegates to the shared <see cref="LunaraDbContext"/> to persist all pending changes.
/// On failure the change tracker is cleared so that retry attempts start from a clean state.
/// A unique-constraint violation on the conversation-per-match index is translated to
/// <see cref="ConversationAlreadyExistsException"/> so the application layer can retry safely.
/// </summary>
internal sealed class EfMessagingUnitOfWork(LunaraDbContext dbContext) : IUnitOfWork
{
    private const string ConversationMatchUniqueIndex = "ix_messaging_conversations_match_id";

    /// <inheritdoc/>
    public async Task SaveChangesAsync(CancellationToken ct)
    {
        try
        {
            await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: ConversationMatchUniqueIndex,
            })
        {
            // A concurrent request inserted the conversation first.
            // Translate to a domain-facing exception so the application layer
            // can retry with the existing conversation, then clear the tracker
            // so the retry starts from a clean state.
            Guid matchId = ex.Entries
                .Select(e => e.Entity)
                .OfType<ConversationEntity>()
                .Select(c => c.MatchId)
                .First();

            dbContext.ChangeTracker.Clear();
            throw new ConversationAlreadyExistsException(new MessagingMatchId(matchId));
        }
        catch
        {
            dbContext.ChangeTracker.Clear();
            throw;
        }
    }
}
