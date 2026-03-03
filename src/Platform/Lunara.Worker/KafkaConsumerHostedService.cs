using System.Text;
using Confluent.Kafka;
using Lunara.Infrastructure.Host.Options;
using Microsoft.Extensions.Options;

namespace Lunara.Worker;

/// <summary>
/// Hosted service that consumes integration events from Kafka topics and dispatches
/// them to the appropriate module handlers.
/// This skeleton implementation logs received events without writing to the database.
/// </summary>
internal sealed partial class KafkaConsumerHostedService(
    IOptions<KafkaOptions> kafkaOptions,
    ILogger<KafkaConsumerHostedService> logger) : BackgroundService
{
    // Retry interval after a non-fatal consume error.
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
                ConsumeResult<string, string>? result =
                    consumer.Consume(stoppingToken);

                if (result is null)
                {
                    continue;
                }

                string topic = result.Topic;
                string key = result.Message.Key ?? string.Empty;
                string value = result.Message.Value ?? string.Empty;
                string? correlationId = ExtractHeader(result.Message.Headers, "correlationId");

                LogEventReceived(logger, topic, key, correlationId);
                LogEventPayload(logger, topic, value);

                // TODO (STEP 8.2+): dispatch to inbox/handler per topic
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

                try
                {
                    await Task.Delay(ErrorBackoff, stoppingToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
            catch (Exception ex)
            {
                LogUnexpectedError(logger, ex, ErrorBackoff);

                try
                {
                    await Task.Delay(ErrorBackoff, stoppingToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
#pragma warning restore CA1031
        }

        consumer.Close();
        LogConsumerStopped(logger);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static string? ExtractHeader(Headers? headers, string name)
    {
        return headers is not null
            && headers.TryGetLastBytes(name, out byte[]? bytes)
            && bytes is not null
            ? Encoding.UTF8.GetString(bytes)
            : null;
    }

    // ── Log messages ──────────────────────────────────────────────────────

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Kafka consumer started. Subscribed to topics: {MatchCreatedTopic}, {MessageSentTopic}.")]
    private static partial void LogConsumerStarted(
        ILogger logger, string matchCreatedTopic, string messageSentTopic);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Kafka event received — topic: {Topic} | key: {Key} | correlationId: {CorrelationId}.")]
    private static partial void LogEventReceived(
        ILogger logger, string topic, string key, string? correlationId);

    [LoggerMessage(Level = LogLevel.Debug,
        Message = "Kafka event payload — topic: {Topic} | value: {Value}.")]
    private static partial void LogEventPayload(
        ILogger logger, string topic, string value);

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
