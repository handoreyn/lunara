namespace Lunara.Moderation.Application.DTOs;

/// <summary>Result of the block-user use case.</summary>
/// <param name="AlreadyBlocked">
///   <c>true</c> when the block already existed and the operation was a no-op.
/// </param>
/// <param name="BlockId">The identifier of the block record, or <c>null</c> when <see cref="AlreadyBlocked"/> is <c>true</c>.</param>
public sealed record BlockUserResult(bool AlreadyBlocked, Guid? BlockId);
