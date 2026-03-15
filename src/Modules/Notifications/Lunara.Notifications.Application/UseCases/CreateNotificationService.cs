using System.Text.Json;
using Lunara.Notifications.Application.Ports;
using Lunara.Notifications.Domain;
using Lunara.Notifications.Domain.Entities;
using Lunara.Notifications.Domain.ValueObjects;

namespace Lunara.Notifications.Application.UseCases;

/// <summary>
/// Creates and persists notifications triggered by domain events from other modules.
/// </summary>
public sealed class CreateNotificationService(
    INotificationRepository repository,
    IUnitOfWork unitOfWork,
    IClock clock)
{
    /// <summary>
    /// Creates one <see cref="NotificationType.MatchCreated"/> notification for each participant
    /// of a new match and persists them.
    /// </summary>
    /// <param name="matchId">The identifier of the newly created match.</param>
    /// <param name="user1Id">The first participant.</param>
    /// <param name="user2Id">The second participant.</param>
    /// <param name="correlationId">Optional correlation identifier for distributed tracing.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>The number of notifications persisted (always 2 on success).</returns>
    public async Task<int> CreateMatchNotificationsAsync(
        Guid matchId,
        Guid user1Id,
        Guid user2Id,
        string? correlationId,
        CancellationToken ct)
    {
        DateTimeOffset now = clock.UtcNow;

        Notification n1 = Notification.Create(
            new NotificationId(Guid.NewGuid()),
            new UserId(user1Id),
            NotificationType.MatchCreated,
            BuildMatchPayload(matchId, otherUserId: user2Id, correlationId),
            now);

        Notification n2 = Notification.Create(
            new NotificationId(Guid.NewGuid()),
            new UserId(user2Id),
            NotificationType.MatchCreated,
            BuildMatchPayload(matchId, otherUserId: user1Id, correlationId),
            now);

        await repository.AddAsync(n1, ct).ConfigureAwait(false);
        await repository.AddAsync(n2, ct).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return 2;
    }

    /// <summary>
    /// Creates a <see cref="NotificationType.MessageReceived"/> notification for the message
    /// recipient and persists it.
    /// </summary>
    /// <param name="conversationId">The conversation in which the message was sent.</param>
    /// <param name="messageId">The identifier of the new message.</param>
    /// <param name="recipientId">The user who should receive the notification.</param>
    /// <param name="payloadJson">
    ///   Caller-provided JSON payload (e.g. message preview). When <see langword="null"/> or
    ///   empty an empty JSON object is stored.
    /// </param>
    /// <param name="correlationId">Optional correlation identifier for distributed tracing.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>The <see cref="NotificationId"/> of the persisted notification.</returns>
    public async Task<NotificationId> CreateMessageReceivedNotificationAsync(
        Guid conversationId,
        Guid messageId,
        Guid recipientId,
        string? payloadJson,
        string? correlationId,
        CancellationToken ct)
    {
        NotificationId id = new(Guid.NewGuid());

        Notification notification = Notification.Create(
            id,
            new UserId(recipientId),
            NotificationType.MessageReceived,
            string.IsNullOrWhiteSpace(payloadJson)
                ? BuildMessagePayload(conversationId, messageId, correlationId)
                : payloadJson,
            clock.UtcNow);

        await repository.AddAsync(notification, ct).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return id;
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static string BuildMatchPayload(Guid matchId, Guid otherUserId, string? correlationId)
    {
        return JsonSerializer.Serialize(new { matchId, otherUserId, correlationId });
    }

    private static string BuildMessagePayload(Guid conversationId, Guid messageId, string? correlationId)
    {
        return JsonSerializer.Serialize(new { conversationId, messageId, correlationId });
    }
}
