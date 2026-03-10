using Lunara.Notifications.Domain.ValueObjects;

namespace Lunara.Notifications.Domain.Entities;

/// <summary>
/// Aggregate root representing a notification delivered to a user.
/// </summary>
public sealed class Notification
{
    /// <summary>Gets the unique identifier of this notification.</summary>
    public NotificationId Id { get; }

    /// <summary>Gets the identifier of the user this notification belongs to.</summary>
    public UserId UserId { get; }

    /// <summary>Gets the type of event that triggered this notification.</summary>
    public NotificationType Type { get; }

    /// <summary>Gets the event-specific payload serialised as a JSON string.</summary>
    public string PayloadJson { get; }

    /// <summary>Gets the UTC timestamp at which this notification was created.</summary>
    public DateTimeOffset CreatedAtUtc { get; }

    /// <summary>
    /// Gets the UTC timestamp at which the user read this notification,
    /// or <see langword="null"/> if it has not yet been read.
    /// </summary>
    public DateTimeOffset? ReadAtUtc { get; private set; }

    private Notification(
        NotificationId id,
        UserId userId,
        NotificationType type,
        string payloadJson,
        DateTimeOffset createdAtUtc,
        DateTimeOffset? readAtUtc = null)
    {
        Id = id;
        UserId = userId;
        Type = type;
        PayloadJson = payloadJson;
        CreatedAtUtc = createdAtUtc;
        ReadAtUtc = readAtUtc;
    }

    /// <summary>
    /// Creates a new <see cref="Notification"/> for the given user and event.
    /// </summary>
    /// <param name="id">The unique identifier for the new notification.</param>
    /// <param name="userId">The recipient user.</param>
    /// <param name="type">The kind of event that triggered the notification.</param>
    /// <param name="payloadJson">Event-specific payload serialised as JSON.</param>
    /// <param name="createdAtUtc">UTC timestamp of creation.</param>
    /// <returns>A new, unread <see cref="Notification"/>.</returns>
    public static Notification Create(
        NotificationId id,
        UserId userId,
        NotificationType type,
        string payloadJson,
        DateTimeOffset createdAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payloadJson);

        return new Notification(id, userId, type, payloadJson, createdAtUtc);
    }

    /// <summary>
    /// Reconstitutes a <see cref="Notification"/> from persisted data without enforcing
    /// creation-time invariants.
    /// </summary>
    /// <param name="id">The persisted notification identifier.</param>
    /// <param name="userId">The owning user identifier.</param>
    /// <param name="type">The persisted notification type.</param>
    /// <param name="payloadJson">The persisted payload JSON string.</param>
    /// <param name="createdAtUtc">The persisted creation timestamp.</param>
    /// <param name="readAtUtc">The persisted read timestamp, or <see langword="null"/>.</param>
    /// <returns>A fully rehydrated <see cref="Notification"/>.</returns>
    public static Notification Reconstitute(
        NotificationId id,
        UserId userId,
        NotificationType type,
        string payloadJson,
        DateTimeOffset createdAtUtc,
        DateTimeOffset? readAtUtc)
    {
        return new Notification(id, userId, type, payloadJson, createdAtUtc, readAtUtc);
    }

    /// <summary>
    /// Marks this notification as read at <paramref name="nowUtc"/>.
    /// Idempotent: has no effect if the notification was already read.
    /// </summary>
    /// <param name="nowUtc">The current UTC time to record as the read timestamp.</param>
    public void MarkRead(DateTimeOffset nowUtc)
    {
        if (ReadAtUtc is not null)
        {
            return;
        }

        ReadAtUtc = nowUtc;
    }
}
