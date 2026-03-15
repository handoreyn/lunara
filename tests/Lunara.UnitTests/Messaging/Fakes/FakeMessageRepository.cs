using Lunara.Messaging.Application.Ports;
using Lunara.Messaging.Domain.Entities;

namespace Lunara.UnitTests.Messaging.Fakes;

/// <summary>In-memory fake for <see cref="IMessageRepository"/> used in unit tests.</summary>
public sealed class FakeMessageRepository : IMessageRepository
{
    private readonly List<Message> _messages = [];

    /// <summary>Gets all messages that have been added.</summary>
    public IReadOnlyList<Message> Messages => _messages;

    /// <inheritdoc/>
    public Task AddAsync(Message message, CancellationToken ct)
    {
        _messages.Add(message);
        return Task.CompletedTask;
    }
}
