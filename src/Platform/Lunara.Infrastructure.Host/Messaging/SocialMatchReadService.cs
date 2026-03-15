using Lunara.Messaging.Application.Ports;
using Lunara.Social.Application.Ports;
using Lunara.Social.Domain.Entities;
using MessagingMatchId = Lunara.Messaging.Domain.ValueObjects.MatchId;
using MessagingUserId = Lunara.Messaging.Domain.ValueObjects.UserId;
using SocialMatchId = Lunara.Social.Domain.ValueObjects.MatchId;

namespace Lunara.Infrastructure.Host.Messaging;

/// <summary>
/// Cross-module adapter that implements <see cref="IMatchReadService"/> by delegating to
/// the Social module's <see cref="IMatchRepository"/>.
/// Translates between the Messaging and Social value objects via their shared underlying
/// <see cref="Guid"/> values.
/// </summary>
internal sealed class SocialMatchReadService(IMatchRepository matchRepository) : IMatchReadService
{
    /// <inheritdoc/>
    public async Task<MatchParticipants?> GetParticipantsAsync(MessagingMatchId matchId, CancellationToken ct)
    {
        SocialMatchId socialMatchId = new(matchId.Value);
        Match? match = await matchRepository
            .FindByIdAsync(socialMatchId, ct)
            .ConfigureAwait(false);

        return match is null
            ? null
            : new MatchParticipants(
                new MessagingUserId(match.User1Id.Value),
                new MessagingUserId(match.User2Id.Value));
    }
}
