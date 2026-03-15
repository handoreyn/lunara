namespace Lunara.Social.Application.DTOs;

/// <summary>Output of the <see cref="UseCases.RecordSwipeService.RecordAsync"/> use-case.</summary>
/// <param name="LikeRecorded">
///   <c>true</c> when a new like was persisted during this call.
/// </param>
/// <param name="MatchCreated">
///   <c>true</c> when a mutual match was formed during this call.
/// </param>
/// <param name="MatchId">
///   The identifier of the newly created match, or <c>null</c> when no match was formed.
/// </param>
/// <param name="Blocked">
///   <c>true</c> when the like was rejected because a block exists in either direction between the users.
/// </param>
public sealed record RecordSwipeResult(
    bool LikeRecorded,
    bool MatchCreated,
    Guid? MatchId,
    bool Blocked = false);
