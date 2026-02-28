using Lunara.Social.Application.Ports;

namespace Lunara.Infrastructure.Host.Social;

/// <summary>
/// Production implementation of <see cref="IClock"/> that delegates to <see cref="DateTimeOffset.UtcNow"/>.
/// </summary>
internal sealed class SystemClock : IClock
{
    /// <inheritdoc/>
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
