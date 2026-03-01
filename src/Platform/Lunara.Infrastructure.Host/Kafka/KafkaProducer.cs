using System.Text;
using Confluent.Kafka;
using Lunara.BuildingBlocks.Kafka;
using Lunara.Infrastructure.Host.Options;
using Microsoft.Extensions.Options;

namespace Lunara.Infrastructure.Host.Kafka;

/// <summary>
/// Confluent.Kafka implementation of <see cref="IKafkaProducer"/>.
/// Registered as a singleton; dispose is handled by the DI container at shutdown.
/// </summary>
internal sealed class KafkaProducer : IKafkaProducer, IDisposable
{
    private readonly IProducer<string, string> _producer;

    /// <summary>
    /// Initialises the Confluent.Kafka producer from <paramref name="options"/>.
    /// </summary>
    public KafkaProducer(IOptions<KafkaOptions> options)
    {
        ProducerConfig config = new()
        {
            BootstrapServers = options.Value.BootstrapServers,
            // Idempotent delivery prevents duplicate messages on retries.
            EnableIdempotence = true,
            Acks = Acks.All,
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    /// <inheritdoc/>
    public async Task PublishAsync(
        string topic,
        string key,
        string valueJson,
        IDictionary<string, string>? headers,
        CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topic);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(valueJson);

        Headers? kafkaHeaders = null;

        if (headers is not null)
        {
            kafkaHeaders = [];

            foreach (KeyValuePair<string, string> header in headers)
            {
                kafkaHeaders.Add(header.Key, Encoding.UTF8.GetBytes(header.Value));
            }
        }

        Message<string, string> message = new()
        {
            Key = key,
            Value = valueJson,
            Headers = kafkaHeaders,
        };

        await _producer.ProduceAsync(topic, message, ct).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }
}
