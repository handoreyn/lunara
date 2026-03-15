using System.Collections.Concurrent;
using Lunara.Messaging.Application.Ports;
using Lunara.Messaging.Domain.Entities;

namespace Lunara.Infrastructure.Host.Messaging;

/// <summary>
/// Thread-safe in-memory implementation of <see cref="IMessageRepository"/> for local development.
/// Data is not persisted across application restarts.
/// </summary>
internal sealed class InMemoryMessageRepository : IMessageRepository
{
    private readonly ConcurrentBag<Message> _messages = [];

    /// <inheritdoc/>
    public Task AddAsync(Message message, CancellationToken ct)
    {
        _messages.Add(message);
        return Task.CompletedTask;
    }
}
