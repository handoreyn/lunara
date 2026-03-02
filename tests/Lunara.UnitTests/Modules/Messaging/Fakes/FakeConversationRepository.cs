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

    /// <summary>
    /// When set, <see cref="AddAsync"/> throws this exception instead of storing the
    /// conversation, and afterwards <see cref="GetByMatchIdAsync"/> returns
    /// <see cref="ConflictConversation"/> to simulate a concurrent-insert race.
    /// </summary>
    public Exception? ThrowOnAdd { get; set; }

    /// <summary>
    /// The conversation that <see cref="GetByMatchIdAsync"/> returns after
    /// <see cref="ThrowOnAdd"/> has been triggered, simulating a concurrent winner.
    /// </summary>
    public Conversation? ConflictConversation { get; set; }

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

        if (ThrowOnAdd is not null)
        {
            Exception toThrow = ThrowOnAdd;
            ThrowOnAdd = null;

            // Simulate the race winner becoming visible before rethrowing.
            if (ConflictConversation is not null)
            {
                _store[ConflictConversation.MatchId.Value] = ConflictConversation;
            }

            throw toThrow;
        }

        _store[conversation.MatchId.Value] = conversation;
        _added.Add(conversation);
        return Task.CompletedTask;
    }
}
