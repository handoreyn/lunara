namespace Lunara.Moderation.Application.DTOs;

/// <summary>Output of the <see cref="UseCases.BlockUserService.BlockAsync"/> use-case.</summary>
/// <param name="BlockCreated">
///   <c>true</c> when a new block was persisted; <c>false</c> when the block already existed (no-op).
/// </param>
public sealed record BlockUserResult(bool BlockCreated);
