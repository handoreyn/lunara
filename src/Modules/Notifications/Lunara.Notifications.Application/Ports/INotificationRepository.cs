using Lunara.Notifications.Domain.Entities;
using Lunara.Notifications.Domain.ValueObjects;

namespace Lunara.Notifications.Application.Ports;

/// <summary>
/// Persistence port for <see cref="Notification"/> aggregates.
/// </summary>
public interface INotificationRepository
{
    /// <summary>
    /// Adds a new notification to the store.
    /// The change is not persisted until <see cref="IUnitOfWork.SaveChangesAsync"/> is called.
    /// </summary>
    /// <param name="notification">The notification to add.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    Task AddAsync(Notification notification, CancellationToken ct);

    /// <summary>
    /// Returns the most recent notifications for a user, newest first.
    /// </summary>
    /// <param name="userId">The target user.</param>
    /// <param name="take">Maximum number of notifications to return.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    Task<IReadOnlyList<Notification>> ListByUserAsync(UserId userId, int take, CancellationToken ct);

    /// <summary>
    /// Returns the notification with the given identifier, or <see langword="null"/> when not found.
    /// </summary>
    /// <param name="id">The notification identifier to look up.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    Task<Notification?> GetByIdAsync(NotificationId id, CancellationToken ct);
}
