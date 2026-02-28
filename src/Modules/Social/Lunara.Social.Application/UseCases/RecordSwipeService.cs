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
    IClock clock)
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

        // Like: check for duplicate.
        bool alreadyLiked = await likeRepository.ExistsLikeAsync(
            request.ActorId, request.TargetId, ct);

        if (alreadyLiked)
        {
            return new RecordSwipeResult(LikeRecorded: false, MatchCreated: false, MatchId: null);
        }

        // Check for reciprocal like before persisting.
        bool reciprocal = await likeRepository.ExistsLikeAsync(
            request.TargetId, request.ActorId, ct);

        await likeRepository.AddLikeAsync(
            request.ActorId, request.TargetId, clock.UtcNow, ct);

        if (reciprocal)
        {
            (Match? match, MatchCreated? _) = Match.CreateMatchIfReciprocalLike(
                request.ActorId, request.TargetId, clock.UtcNow, reciprocalLikeExists: true);

            // match is never null here because reciprocal=true and actor≠target,
            // but we null-check defensively to keep the compiler happy.
            if (match is not null)
            {
                await matchRepository.AddAsync(match, ct);
                await unitOfWork.SaveChangesAsync(ct);
                return new RecordSwipeResult(LikeRecorded: true, MatchCreated: true, MatchId: match.Id.Value);
            }
        }

        await unitOfWork.SaveChangesAsync(ct);
        return new RecordSwipeResult(LikeRecorded: true, MatchCreated: false, MatchId: null);
    }
}
