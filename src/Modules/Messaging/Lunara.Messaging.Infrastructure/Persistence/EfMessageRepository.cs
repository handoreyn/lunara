using Lunara.Messaging.Application.Ports;
using Lunara.Messaging.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lunara.Messaging.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of <see cref="IMessageRepository"/>.
/// Receives the shared <see cref="DbContext"/> from the DI container and accesses
/// <see cref="MessageEntity"/> rows via <see cref="DbContext.Set{T}()"/>.
/// </summary>
internal sealed class EfMessageRepository(DbContext dbContext) : IMessageRepository
{
    /// <inheritdoc/>
    public async Task AddAsync(Message message, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(message);

        MessageEntity entity = ToEntity(message);
        await dbContext.Set<MessageEntity>().AddAsync(entity, ct).ConfigureAwait(false);
    }

    // ── Mapping ────────────────────────────────────────────────────────────

    private static MessageEntity ToEntity(Message m)
    {
        return new MessageEntity
        {
            Id = m.Id.Value,
            ConversationId = m.ConversationId.Value,
            SenderId = m.SenderId.Value,
            RecipientId = m.RecipientId.Value,
            Text = m.Text,
            SentAtUtc = m.SentAtUtc,
        };
    }
}
