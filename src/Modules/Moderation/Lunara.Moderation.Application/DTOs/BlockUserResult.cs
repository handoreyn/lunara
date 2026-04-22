namespace Lunara.Moderation.Application.DTOs;

/// <summary>Result returned by the block-user use case.</summary>
/// <param name="BlockId">
///   The identifier of the newly created block, or <c>null</c> if the block already existed
///   (duplicate block is a no-op).
/// </param>
/// <param name="AlreadyBlocked">
///   <c>true</c> when a block already existed and no new block was created.
/// </param>
public sealed record BlockUserResult(Guid? BlockId, bool AlreadyBlocked);
