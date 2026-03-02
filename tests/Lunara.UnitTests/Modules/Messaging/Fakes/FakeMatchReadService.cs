using Lunara.Messaging.Application.Ports;
using Lunara.Messaging.Domain.ValueObjects;

namespace Lunara.UnitTests.Modules.Messaging.Fakes;

/// <summary>
/// In-memory fake for <see cref="IMatchReadService"/> used in Messaging unit tests.
/// </summary>
public sealed class FakeMatchReadService : IMatchReadService
{
    private readonly Dictionary<Guid, MatchParticipants> _matches = [];

    /// <summary>Seeds a match with its canonical participant pair.</summary>
    /// <param name="matchId">The match to register.</param>
    /// <param name="user1Id">The participant with the lexicographically smaller identifier.</param>
    /// <param name="user2Id">The participant with the lexicographically larger identifier.</param>
    public void SeedMatch(MatchId matchId, UserId user1Id, UserId user2Id)
    {
        _matches[matchId.Value] = new MatchParticipants(user1Id, user2Id);
    }

    /// <inheritdoc/>
    public Task<MatchParticipants?> GetParticipantsAsync(MatchId matchId, CancellationToken ct)
    {
        _matches.TryGetValue(matchId.Value, out MatchParticipants? participants);
        return Task.FromResult(participants);
    }
}
