namespace Lunara.BuildingBlocks.Kafka;

/// <summary>
/// Publishes messages to a Kafka topic.
/// Implementations are expected to be thread-safe and registered as singletons.
/// </summary>
public interface IKafkaProducer
{
    /// <summary>
    /// Publishes a single message to the specified Kafka topic and waits for broker acknowledgement.
    /// </summary>
    /// <param name="topic">The target Kafka topic name.</param>
    /// <param name="key">The message key used for partition routing.</param>
    /// <param name="valueJson">The message value serialised as a JSON string.</param>
    /// <param name="headers">Optional key-value headers to attach to the message.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    Task PublishAsync(
        string topic,
        string key,
        string valueJson,
        IDictionary<string, string>? headers,
        CancellationToken ct);
}
