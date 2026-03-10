using Lunara.Notifications.Application.Ports;
using Lunara.Notifications.Domain;
using Lunara.Notifications.Domain.Entities;
using Lunara.Notifications.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Lunara.Notifications.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of <see cref="INotificationRepository"/>.
/// Receives the shared <see cref="DbContext"/> from the DI container and accesses
/// <see cref="NotificationEntity"/> rows via <see cref="DbContext.Set{T}()"/>.
/// </summary>
internal sealed class EfNotificationRepository(DbContext dbContext) : INotificationRepository
{
    /// <inheritdoc/>
    public async Task AddAsync(Notification notification, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(notification);

        NotificationEntity entity = ToEntity(notification);
        await dbContext.Set<NotificationEntity>().AddAsync(entity, ct).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Notification>> ListByUserAsync(
        UserId userId,
        int take,
        CancellationToken ct)
    {
        List<NotificationEntity> rows = await dbContext.Set<NotificationEntity>()
            .AsNoTracking()
            .Where(e => e.UserId == userId.Value)
            .OrderByDescending(e => e.CreatedAtUtc)
            .Take(take)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        return rows.ConvertAll(ToDomain);
    }

    /// <inheritdoc/>
    public async Task<Notification?> GetByIdAsync(NotificationId id, CancellationToken ct)
    {
        NotificationEntity? entity = await dbContext.Set<NotificationEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id.Value, ct)
            .ConfigureAwait(false);

        return entity is null ? null : ToDomain(entity);
    }

    // ── Mapping ────────────────────────────────────────────────────────────

    private static NotificationEntity ToEntity(Notification n)
    {
        return new NotificationEntity
        {
            Id = n.Id.Value,
            UserId = n.UserId.Value,
            Type = (int)n.Type,
            PayloadJson = n.PayloadJson,
            CreatedAtUtc = n.CreatedAtUtc,
            ReadAtUtc = n.ReadAtUtc,
        };
    }

    private static Notification ToDomain(NotificationEntity e)
    {
        return Notification.Reconstitute(
            id: new NotificationId(e.Id),
            userId: new UserId(e.UserId),
            type: (NotificationType)e.Type,
            payloadJson: e.PayloadJson,
            createdAtUtc: e.CreatedAtUtc,
            readAtUtc: e.ReadAtUtc);
    }
}
