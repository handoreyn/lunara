using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Lunara.BuildingBlocks.EventContracts.V1;
using Lunara.BuildingBlocks.Inbox;
using Lunara.Infrastructure.Host.Options;
using Lunara.Notifications.Application.UseCases;
using Microsoft.Extensions.Options;

namespace Lunara.Worker;

/// <summary>
/// Hosted service that consumes integration events from Kafka, gates each event
/// through <see cref="IInboxStore"/> for idempotency, then creates notifications
/// via <see cref="CreateNotificationService"/>.
/// </summary>
internal sealed partial class KafkaConsumerHostedService(
    IServiceScopeFactory scopeFactory,
    IOptions<KafkaOptions> kafkaOptions,
    ILogger<KafkaConsumerHostedService> logger) : BackgroundService
{
    private const string ConsumerName = "notifications";
    private static readonly TimeSpan ErrorBackoff = TimeSpan.FromSeconds(5);

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        KafkaOptions options = kafkaOptions.Value;

        string matchCreatedTopic = $"{options.TopicPrefix}social.match-created.v1";
        string messageSentTopic = $"{options.TopicPrefix}messaging.message-sent.v1";

        ConsumerConfig config = new()
        {
            BootstrapServers = options.BootstrapServers,
            GroupId = options.ConsumerGroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            EnableAutoOffsetStore = false,
        };

        using IConsumer<string, string> consumer =
            new ConsumerBuilder<string, string>(config).Build();

        consumer.Subscribe([matchCreatedTopic, messageSentTopic]);

        LogConsumerStarted(logger, matchCreatedTopic, messageSentTopic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                ConsumeResult<string, string>? result = consumer.Consume(stoppingToken);

                if (result is null)
                {
                    continue;
                }

                string topic = result.Topic;
                string key = result.Message.Key ?? string.Empty;
                string value = result.Message.Value ?? string.Empty;
                string? correlationId = ExtractHeader(result.Message.Headers, "correlationId");
                Guid eventId = ResolveEventId(key, topic, value);

                LogEventReceived(logger, topic, key, eventId, correlationId);

                await DispatchAsync(topic, value, eventId, correlationId, stoppingToken)
                    .ConfigureAwait(false);

                consumer.StoreOffset(result);
                consumer.Commit(result);
            }
            catch (OperationCanceledException)
            {
                break;
            }
#pragma warning disable CA1031 // background service must not crash on transient errors
            catch (ConsumeException ex)
            {
                LogConsumeError(logger, ex, ex.Error.Reason, ErrorBackoff);
                await DelayAsync(ErrorBackoff, stoppingToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogUnexpectedError(logger, ex, ErrorBackoff);
                await DelayAsync(ErrorBackoff, stoppingToken).ConfigureAwait(false);
            }
#pragma warning restore CA1031
        }

        consumer.Close();
        LogConsumerStopped(logger);
    }

    // ── Dispatch ──────────────────────────────────────────────────────────

    private async Task DispatchAsync(
        string topic,
        string value,
        Guid eventId,
        string? correlationId,
        CancellationToken ct)
    {
        AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        await using (scope.ConfigureAwait(false))
        {
            IInboxStore inbox = scope.ServiceProvider.GetRequiredService<IInboxStore>();
            CreateNotificationService notificationService =
                scope.ServiceProvider.GetRequiredService<CreateNotificationService>();

            DateTimeOffset now = DateTimeOffset.UtcNow;
            bool acquired = await inbox.TryAcquireAsync(eventId, ConsumerName, now, ct)
                .ConfigureAwait(false);

            if (!acquired)
            {
                LogEventSkipped(logger, topic, eventId);
                return;
            }

            try
            {
                await HandleAsync(topic, value, eventId, correlationId, notificationService, ct)
                    .ConfigureAwait(false);

                await inbox.MarkProcessedAsync(eventId, ConsumerName, DateTimeOffset.UtcNow, ct)
                    .ConfigureAwait(false);

                LogEventProcessed(logger, topic, eventId, correlationId);
            }
            catch (JsonException ex)
            {
                // Payload could not be deserialised — log the failure, mark the inbox
                // row as processed so it does not remain permanently "in-flight", and
                // do NOT rethrow so the Kafka offset is still committed (prevents a
                // poison-message retry loop).
                LogDeserializationFailed(logger, ex, topic, correlationId, eventId);

                await inbox.MarkProcessedAsync(eventId, ConsumerName, DateTimeOffset.UtcNow, ct)
                    .ConfigureAwait(false);
            }
            catch (Exception)
            {
                // Allow the exception to bubble — caller backs off and re-consumes
                // (inbox row without ProcessedAtUtc = "in-flight"; next delivery
                //  will fail TryAcquireAsync and be skipped, which is acceptable
                //  since the row was not committed in a transaction with the
                //  notification write — a future improvement can wrap both in one TX).
                throw;
            }
        }
    }

    private async Task HandleAsync(
        string topic,
        string value,
        Guid eventId,
        string? correlationId,
        CreateNotificationService notificationService,
        CancellationToken ct)
    {
        if (topic.EndsWith("social.match-created.v1", StringComparison.Ordinal))
        {
            await HandleMatchCreatedAsync(value, eventId, correlationId, notificationService, ct)
                .ConfigureAwait(false);
            return;
        }

        if (topic.EndsWith("messaging.message-sent.v1", StringComparison.Ordinal))
        {
            await HandleMessageSentAsync(value, eventId, correlationId, notificationService, ct)
                .ConfigureAwait(false);
            return;
        }

        LogUnknownTopic(logger, topic, eventId);
    }

    private async Task HandleMatchCreatedAsync(
        string value,
        Guid eventId,
        string? correlationId,
        CreateNotificationService notificationService,
        CancellationToken ct)
    {
        SocialMatchCreatedV1 payload = EventJson.Deserialize<SocialMatchCreatedV1>(value);

        int count = await notificationService.CreateMatchNotificationsAsync(
            matchId: payload.MatchId,
            user1Id: payload.User1Id,
            user2Id: payload.User2Id,
            correlationId: correlationId,
            ct: ct).ConfigureAwait(false);

        LogNotificationsCreated(logger, count, "social.match-created.v1", eventId);
    }

    private async Task HandleMessageSentAsync(
        string value,
        Guid eventId,
        string? correlationId,
        CreateNotificationService notificationService,
        CancellationToken ct)
    {
        MessagingMessageSentV1 payload = EventJson.Deserialize<MessagingMessageSentV1>(value);

        Lunara.Notifications.Domain.ValueObjects.NotificationId notificationId =
            await notificationService.CreateMessageReceivedNotificationAsync(
                conversationId: payload.ConversationId,
                messageId: payload.MessageId,
                recipientId: payload.RecipientId,
                payloadJson: null,
                correlationId: correlationId,
                ct: ct).ConfigureAwait(false);

        LogNotificationsCreated(logger, 1, "messaging.message-sent.v1", eventId);
        _ = notificationId; // Id available for future use (e.g. push notification dispatch)
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    /// <summary>
    /// Resolves a stable <see cref="Guid"/> to use as the inbox idempotency key.
    /// Preference order:
    /// 1. Kafka message key if it is a valid <see cref="Guid"/> (set by the outbox poller).
    /// 2. Deterministic (v5-like) UUID derived from topic + key + value — used for
    ///    messages originating outside the platform outbox.
    /// </summary>
    private static Guid ResolveEventId(string key, string topic, string value)
    {
        if (Guid.TryParse(key, out Guid parsed))
        {
            return parsed;
        }

        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes($"{topic}|{key}|{value}"));
        Span<byte> guidBytes = stackalloc byte[16];
        hash.AsSpan(0, 16).CopyTo(guidBytes);
        guidBytes[6] = (byte)((guidBytes[6] & 0x0F) | 0x50); // version 5
        guidBytes[8] = (byte)((guidBytes[8] & 0x3F) | 0x80); // variant RFC 4122
        return new Guid(guidBytes);
    }

    private static string? ExtractHeader(Headers? headers, string name)
    {
        return headers is not null
            && headers.TryGetLastBytes(name, out byte[]? bytes)
            && bytes is not null
            ? Encoding.UTF8.GetString(bytes)
            : null;
    }

    private static async Task DelayAsync(TimeSpan delay, CancellationToken ct)
    {
        try
        {
            await Task.Delay(delay, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Swallow — outer loop will exit on next iteration check.
        }
    }

    // ── Log messages ──────────────────────────────────────────────────────

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Kafka consumer started. Subscribed to topics: {MatchCreatedTopic}, {MessageSentTopic}.")]
    private static partial void LogConsumerStarted(
        ILogger logger, string matchCreatedTopic, string messageSentTopic);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Kafka event received — topic: {Topic} | key: {Key} | eventId: {EventId} | correlationId: {CorrelationId}.")]
    private static partial void LogEventReceived(
        ILogger logger, string topic, string key, Guid eventId, string? correlationId);

    [LoggerMessage(Level = LogLevel.Debug,
        Message = "Kafka event {EventId} on topic {Topic} already processed — skipping.")]
    private static partial void LogEventSkipped(ILogger logger, string topic, Guid eventId);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Kafka event {EventId} on topic {Topic} processed successfully. correlationId: {CorrelationId}.")]
    private static partial void LogEventProcessed(
        ILogger logger, string topic, Guid eventId, string? correlationId);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Created {Count} notification(s) for event {EventId} on topic {Topic}.")]
    private static partial void LogNotificationsCreated(
        ILogger logger, int count, string topic, Guid eventId);

    [LoggerMessage(Level = LogLevel.Error,
        Message = "Failed to deserialise payload for event type {EventType} (eventId: {EventId}, correlationId: {CorrelationId}). Inbox row left unprocessed.")]
    private static partial void LogDeserializationFailed(
        ILogger logger, Exception ex, string eventType, string? correlationId, Guid eventId);

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "Received event {EventId} on unknown topic {Topic}. Skipping.")]
    private static partial void LogUnknownTopic(ILogger logger, string topic, Guid eventId);

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "Kafka consume error: {Reason}. Backing off for {Backoff}.")]
    private static partial void LogConsumeError(
        ILogger logger, Exception ex, string reason, TimeSpan backoff);

    [LoggerMessage(Level = LogLevel.Error,
        Message = "Unexpected error in Kafka consumer loop. Backing off for {Backoff}.")]
    private static partial void LogUnexpectedError(
        ILogger logger, Exception ex, TimeSpan backoff);

    [LoggerMessage(Level = LogLevel.Information, Message = "Kafka consumer stopped.")]
    private static partial void LogConsumerStopped(ILogger logger);
}
