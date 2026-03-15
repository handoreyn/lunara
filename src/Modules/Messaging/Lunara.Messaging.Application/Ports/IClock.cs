namespace Lunara.Messaging.Application.Ports;

/// <summary>Abstracts the system clock to allow deterministic time in tests.</summary>
public interface IClock
{
    /// <summary>Gets the current UTC date and time.</summary>
    DateTimeOffset UtcNow { get; }
}
