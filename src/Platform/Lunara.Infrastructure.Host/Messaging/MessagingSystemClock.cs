using Lunara.Messaging.Application.Ports;

namespace Lunara.Infrastructure.Host.Messaging;

/// <summary>
/// Production implementation of <see cref="IClock"/> that delegates to <see cref="DateTimeOffset.UtcNow"/>.
/// </summary>
internal sealed class MessagingSystemClock : IClock
{
    /// <inheritdoc/>
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
