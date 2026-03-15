using Lunara.Moderation.Domain.ValueObjects;

namespace Lunara.Moderation.Application.DTOs;

/// <summary>Input for the <see cref="UseCases.BlockUserService.BlockAsync"/> use-case.</summary>
/// <param name="BlockerUserId">The user initiating the block.</param>
/// <param name="BlockedUserId">The user to block.</param>
public sealed record BlockUserRequest(UserId BlockerUserId, UserId BlockedUserId);
