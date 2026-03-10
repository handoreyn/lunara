namespace Lunara.BuildingBlocks.EventContracts.V1;

/// <summary>
/// Integration event emitted when two users are matched in the Social module.
/// </summary>
/// <param name="MatchId">Unique identifier of the newly created match.</param>
/// <param name="User1Id">Identifier of the first participant in the match.</param>
/// <param name="User2Id">Identifier of the second participant in the match.</param>
/// <param name="OccurredAtUtc">UTC timestamp at which the match was created.</param>
public sealed record SocialMatchCreatedV1(
    Guid MatchId,
    Guid User1Id,
    Guid User2Id,
    DateTimeOffset OccurredAtUtc);
