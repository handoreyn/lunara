using Lunara.Messaging.Application.Ports;

namespace Lunara.Infrastructure.Host.Messaging;

/// <summary>
/// No-op implementation of <see cref="IUnitOfWork"/> for local development.
/// In-memory repositories commit changes immediately, so no explicit flush is required.
/// </summary>
internal sealed class InMemoryMessagingUnitOfWork : IUnitOfWork
{
    /// <inheritdoc/>
    public Task SaveChangesAsync(CancellationToken ct)
    {
        return Task.CompletedTask;
    }
}
