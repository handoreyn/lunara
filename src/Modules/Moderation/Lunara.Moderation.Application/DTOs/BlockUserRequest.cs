using Lunara.Moderation.Domain.ValueObjects;

namespace Lunara.Moderation.Application.DTOs;

/// <summary>Input data for the block-user use case.</summary>
/// <param name="BlockerUserId">The user initiating the block.</param>
/// <param name="BlockedUserId">The user to be blocked.</param>
public sealed record BlockUserRequest(UserId BlockerUserId, UserId BlockedUserId);
