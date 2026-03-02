using Lunara.Messaging.Application.Ports;
using Lunara.Social.Application.Ports;
using MessagingMatchId = Lunara.Messaging.Domain.ValueObjects.MatchId;
using SocialMatchId = Lunara.Social.Domain.ValueObjects.MatchId;

namespace Lunara.Infrastructure.Host.Messaging;

/// <summary>
/// Cross-module adapter that implements <see cref="IMatchReadService"/> by delegating to
/// the Social module's <see cref="IMatchRepository"/>.
/// Translates between the Messaging and Social <c>MatchId</c> value objects via their
/// shared underlying <see cref="Guid"/> value.
/// </summary>
internal sealed class SocialMatchReadService(IMatchRepository matchRepository) : IMatchReadService
{
    /// <inheritdoc/>
    public Task<bool> MatchExistsAsync(MessagingMatchId matchId, CancellationToken ct)
    {
        SocialMatchId socialMatchId = new(matchId.Value);
        return matchRepository.ExistsAsync(socialMatchId, ct);
    }
}
