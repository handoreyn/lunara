namespace Lunara.Messaging.Application.Ports;

/// <summary>Abstraction over the system clock to allow deterministic testing.</summary>
public interface IClock
{
    /// <summary>Gets the current UTC time.</summary>
    DateTimeOffset UtcNow { get; }
}
