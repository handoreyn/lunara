using Lunara.Messaging.Application.Ports;
using Lunara.Messaging.Domain.ValueObjects;

namespace Lunara.UnitTests.Modules.Messaging.Fakes;

/// <summary>
/// In-memory fake for <see cref="IMatchReadService"/> used in Messaging unit tests.
/// </summary>
public sealed class FakeMatchReadService : IMatchReadService
{
    private readonly HashSet<Guid> _existingMatchIds = [];

    /// <summary>Seeds a match id that will be returned as existing.</summary>
    /// <param name="matchId">The match to register as existing.</param>
    public void SeedMatch(MatchId matchId)
    {
        _existingMatchIds.Add(matchId.Value);
    }

    /// <inheritdoc/>
    public Task<bool> MatchExistsAsync(MatchId matchId, CancellationToken ct)
    {
        return Task.FromResult(_existingMatchIds.Contains(matchId.Value));
    }
}
