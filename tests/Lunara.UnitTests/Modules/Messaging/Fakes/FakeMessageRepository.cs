using Lunara.Messaging.Application.Ports;
using Lunara.Messaging.Domain.Entities;

namespace Lunara.UnitTests.Modules.Messaging.Fakes;

/// <summary>
/// In-memory fake for <see cref="IMessageRepository"/> used in Messaging unit tests.
/// </summary>
public sealed class FakeMessageRepository : IMessageRepository
{
    private readonly List<Message> _added = [];

    /// <summary>Gets all messages that were passed to <see cref="AddAsync"/>.</summary>
    public IReadOnlyList<Message> AddedMessages => _added;

    /// <summary>Gets the number of times <see cref="AddAsync"/> was called.</summary>
    public int AddCallCount { get; private set; }

    /// <inheritdoc/>
    public Task AddAsync(Message message, CancellationToken ct)
    {
        AddCallCount++;
        _added.Add(message);
        return Task.CompletedTask;
    }
}
