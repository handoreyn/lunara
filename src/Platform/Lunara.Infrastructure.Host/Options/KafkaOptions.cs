namespace Lunara.Infrastructure.Host.Options;

/// <summary>Apache Kafka broker connectivity and topic-naming options.</summary>
public sealed class KafkaOptions
{
    /// <summary>The configuration section key used when binding from <c>appsettings.json</c>.</summary>
    public const string SectionKey = "Kafka";

    /// <summary>
    /// Gets or sets the comma-separated list of Kafka bootstrap servers
    /// (e.g. <c>"localhost:9092"</c>).
    /// </summary>
    public string BootstrapServers { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional prefix that is prepended to every topic name
    /// (e.g. <c>"lunara."</c>).
    /// </summary>
    public string TopicPrefix { get; set; } = string.Empty;
}
