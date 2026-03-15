using Lunara.Moderation.Application.Ports;

namespace Lunara.Infrastructure.Host.Moderation;

/// <summary>
/// Production implementation of <see cref="IClock"/> for the Moderation module.
/// Delegates to <see cref="DateTimeOffset.UtcNow"/>.
/// </summary>
internal sealed class ModerationSystemClock : IClock
{
    /// <inheritdoc/>
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
