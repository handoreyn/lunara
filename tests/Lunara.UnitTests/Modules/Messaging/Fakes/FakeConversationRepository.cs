using Lunara.Messaging.Application.Ports;
using Lunara.Messaging.Domain.Entities;
using Lunara.Messaging.Domain.ValueObjects;

namespace Lunara.UnitTests.Modules.Messaging.Fakes;

/// <summary>
/// In-memory fake for <see cref="IConversationRepository"/> used in Messaging unit tests.
/// </summary>
public sealed class FakeConversationRepository : IConversationRepository
{
    private readonly Dictionary<Guid, Conversation> _store = [];
    private readonly List<Conversation> _added = [];

    /// <summary>Gets all conversations that were passed to <see cref="AddAsync"/>.</summary>
    public IReadOnlyList<Conversation> AddedConversations => _added.AsReadOnly();

    /// <summary>Gets the number of times <see cref="AddAsync"/> was called.</summary>
    public int AddCallCount { get; private set; }

    /// <summary>Seeds an existing conversation so <see cref="GetByMatchIdAsync"/> finds it.</summary>
    /// <param name="conversation">The conversation to pre-populate.</param>
    public void Seed(Conversation conversation)
    {
        ArgumentNullException.ThrowIfNull(conversation);
        _store[conversation.MatchId.Value] = conversation;
    }

    /// <inheritdoc/>
    public Task<Conversation?> GetByMatchIdAsync(MatchId matchId, CancellationToken ct)
    {
        _store.TryGetValue(matchId.Value, out Conversation? conversation);
        return Task.FromResult(conversation);
    }

    /// <inheritdoc/>
    public Task AddAsync(Conversation conversation, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(conversation);
        AddCallCount++;

        // Do NOT write to _store here — that would simulate a committed DB row,
        // but AddAsync only stages the entity for the next SaveChangesAsync.
        // Tests that need the conversation visible after a save should call Seed().
        _added.Add(conversation);
        return Task.CompletedTask;
    }
}
