using Lunara.Messaging.Domain.ValueObjects;

namespace Lunara.Messaging.Application.DTOs;

/// <summary>Input for the <see cref="UseCases.SendMessageService.SendAsync"/> use-case.</summary>
/// <param name="MatchId">The match that owns the conversation.</param>
/// <param name="SenderId">The user sending the message.</param>
/// <param name="RecipientId">
///   The other participant. Used to bootstrap a new <see cref="Domain.Entities.Conversation"/>
///   when one does not yet exist for this match.
/// </param>
/// <param name="Text">The raw message text (will be trimmed by the domain).</param>
/// <param name="CorrelationId">
///   Optional correlation identifier propagated from the HTTP layer (e.g. <c>X-Correlation-Id</c>)
///   for distributed tracing.
/// </param>
public sealed record SendMessageRequest(
    MatchId MatchId,
    UserId SenderId,
    UserId RecipientId,
    string Text,
    string? CorrelationId);
