using Lunara.Messaging.Application.Ports;
using Lunara.Messaging.Domain.ValueObjects;

namespace Lunara.Infrastructure.Host.Messaging;

/// <summary>
/// Permissive stub for <see cref="IMatchReadService"/> used during local development
/// while the Social module still operates on in-memory storage.
/// Always returns <c>true</c> so that the Messaging use-case proceeds without a live Social store.
/// </summary>
/// <remarks>Replace with a real cross-module adapter once Social gains EF persistence.</remarks>
internal sealed class StubMatchReadService : IMatchReadService
{
    /// <inheritdoc/>
    public Task<bool> MatchExistsAsync(MatchId matchId, CancellationToken ct)
    {
        return Task.FromResult(true);
    }
}
