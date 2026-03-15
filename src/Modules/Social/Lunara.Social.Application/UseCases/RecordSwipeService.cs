using Lunara.BuildingBlocks.EventContracts.V1;
using Lunara.BuildingBlocks.Moderation;
using Lunara.BuildingBlocks.Outbox;
using Lunara.Social.Application.DTOs;
using Lunara.Social.Application.Ports;
using Lunara.Social.Domain;
using Lunara.Social.Domain.DomainEvents;
using Lunara.Social.Domain.Entities;

namespace Lunara.Social.Application.UseCases;

/// <summary>
/// Records the outcome of a user swiping on another user's profile,
/// creating a mutual match when both parties have liked each other.
/// </summary>
public sealed class RecordSwipeService(
    ILikeRepository likeRepository,
    IMatchRepository matchRepository,
    IUnitOfWork unitOfWork,
    IClock clock,
    IOutboxWriter outboxWriter,
    IBlockChecker blockChecker)
{
    /// <summary>
    /// Processes a swipe and returns the outcome.
    /// </summary>
    /// <param name="request">The swipe details.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>
    ///   A <see cref="RecordSwipeResult"/> describing whether a like and/or match was created.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   Thrown when <paramref name="request"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   Thrown when <paramref name="request"/> has equal <c>ActorId</c> and <c>TargetId</c>.
    /// </exception>
    public async Task<RecordSwipeResult> RecordAsync(
        RecordSwipeRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.ActorId == request.TargetId)
        {
            throw new ArgumentException(
                "Actor and target must be different users.",
                nameof(request));
        }

        // Pass: nothing to persist.
        if (request.Action == SwipeActionType.Pass)
        {
            return new RecordSwipeResult(LikeRecorded: false, MatchCreated: false, MatchId: null);
        }

        // Block check: if either user has blocked the other, swipe is a no-op.
        bool blocked = await blockChecker
            .IsBlockedAsync(request.ActorId.Value, request.TargetId.Value, ct)
            .ConfigureAwait(false);

        if (blocked)
        {
            return new RecordSwipeResult(LikeRecorded: false, MatchCreated: false, MatchId: null);
        }

        // Like: check for duplicate.
        bool alreadyLiked = await likeRepository.ExistsLikeAsync(
            request.ActorId, request.TargetId, ct)
            .ConfigureAwait(false);

        if (alreadyLiked)
        {
            return new RecordSwipeResult(LikeRecorded: false, MatchCreated: false, MatchId: null);
        }

        DateTimeOffset now = clock.UtcNow;

        // Check for reciprocal like before persisting.
        bool reciprocal = await likeRepository.ExistsLikeAsync(
            request.TargetId, request.ActorId, ct)
            .ConfigureAwait(false);

        await likeRepository.AddLikeAsync(
            request.ActorId, request.TargetId, now, ct)
            .ConfigureAwait(false);

        if (reciprocal)
        {
            (Match? match, MatchCreated? _) = Match.CreateMatchIfReciprocalLike(
                request.ActorId, request.TargetId, now, reciprocalLikeExists: true);

            // match is never null here because reciprocal=true and actor≠target,
            // but we null-check defensively to keep the compiler happy.
            if (match is not null)
            {
                await matchRepository.AddAsync(match, ct).ConfigureAwait(false);

                string payloadJson = EventJson.Serialize(new SocialMatchCreatedV1(
                    MatchId: match.Id.Value,
                    User1Id: match.User1Id.Value,
                    User2Id: match.User2Id.Value,
                    OccurredAtUtc: now));

                await outboxWriter.EnqueueAsync(
                    "social.match-created.v1",
                    payloadJson,
                    now,
                    request.CorrelationId,
                    ct)
                    .ConfigureAwait(false);

                await unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);
                return new RecordSwipeResult(LikeRecorded: true, MatchCreated: true, MatchId: match.Id.Value);
            }
        }

        await unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);
        return new RecordSwipeResult(LikeRecorded: true, MatchCreated: false, MatchId: null);
    }
}
