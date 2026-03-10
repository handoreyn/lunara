namespace Lunara.Notifications.Infrastructure.Persistence;

/// <summary>
/// EF Core persistence row for a <see cref="Domain.Entities.Notification"/> aggregate.
/// All columns are raw primitives; no domain value objects cross the persistence boundary.
/// </summary>
public sealed class NotificationEntity
{
    /// <summary>Gets or sets the unique identifier of the notification.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the identifier of the user this notification belongs to.</summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the discriminator for the notification type stored as an integer.
    /// Maps to <see cref="Domain.NotificationType"/>.
    /// </summary>
    public int Type { get; set; }

    /// <summary>Gets or sets the event-specific payload serialised as a JSON string.</summary>
    public string PayloadJson { get; set; } = string.Empty;

    /// <summary>Gets or sets the UTC timestamp at which this notification was created.</summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp at which the user read this notification,
    /// or <see langword="null"/> if it has not been read yet.
    /// </summary>
    public DateTimeOffset? ReadAtUtc { get; set; }
}
