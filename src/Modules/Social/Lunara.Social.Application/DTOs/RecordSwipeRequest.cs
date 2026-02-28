using Lunara.Social.Domain;
using Lunara.Social.Domain.ValueObjects;

namespace Lunara.Social.Application.DTOs;

/// <summary>Input for the <see cref="UseCases.RecordSwipeService.RecordAsync"/> use-case.</summary>
/// <param name="ActorId">The user performing the swipe.</param>
/// <param name="TargetId">The user being swiped on.</param>
/// <param name="Action">The swipe action taken.</param>
public sealed record RecordSwipeRequest(
    UserId ActorId,
    UserId TargetId,
    SwipeActionType Action)
{
    /// <summary>
    /// Gets or inits an optional correlation identifier propagated from the HTTP layer
    /// (e.g. the <c>X-Correlation-Id</c> request header) for distributed tracing.
    /// </summary>
    public string? CorrelationId { get; init; }
}
