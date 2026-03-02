using Lunara.Messaging.Application.Ports;
using Lunara.Messaging.Domain.Entities;
using Lunara.Messaging.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Lunara.Messaging.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of <see cref="IConversationRepository"/>.
/// Receives the shared <see cref="DbContext"/> from the DI container and accesses
/// <see cref="ConversationEntity"/> rows via <see cref="DbContext.Set{T}()"/>.
/// </summary>
internal sealed class EfConversationRepository(DbContext dbContext) : IConversationRepository
{
    /// <inheritdoc/>
    public async Task<Conversation?> GetByMatchIdAsync(MatchId matchId, CancellationToken ct)
    {
        ConversationEntity? entity = await dbContext.Set<ConversationEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.MatchId == matchId.Value, ct)
            .ConfigureAwait(false);

        return entity is null ? null : ToDomain(entity);
    }

    /// <inheritdoc/>
    public async Task AddAsync(Conversation conversation, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(conversation);

        ConversationEntity entity = ToEntity(conversation);
        await dbContext.Set<ConversationEntity>().AddAsync(entity, ct).ConfigureAwait(false);
    }

    // ── Mapping ────────────────────────────────────────────────────────────

    private static ConversationEntity ToEntity(Conversation c)
    {
        return new ConversationEntity
        {
            Id = c.Id.Value,
            MatchId = c.MatchId.Value,
            User1Id = c.User1Id.Value,
            User2Id = c.User2Id.Value,
            CreatedAtUtc = c.CreatedAtUtc,
        };
    }

    private static Conversation ToDomain(ConversationEntity e)
    {
        return Conversation.Reconstitute(
            id: new ConversationId(e.Id),
            matchId: new MatchId(e.MatchId),
            user1Id: new UserId(e.User1Id),
            user2Id: new UserId(e.User2Id),
            createdAtUtc: e.CreatedAtUtc);
    }
}
