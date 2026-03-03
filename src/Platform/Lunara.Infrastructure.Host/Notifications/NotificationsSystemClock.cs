using Lunara.Notifications.Application.Ports;

namespace Lunara.Infrastructure.Host.Notifications;

/// <summary>
/// Production implementation of the Notifications module <see cref="IClock"/>
/// that delegates to <see cref="DateTimeOffset.UtcNow"/>.
/// </summary>
internal sealed class NotificationsSystemClock : IClock
{
    /// <inheritdoc/>
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
