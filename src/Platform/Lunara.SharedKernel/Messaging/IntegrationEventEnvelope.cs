using System.Text.Json;

namespace Lunara.SharedKernel.Messaging;

/// <summary>
/// Envelope that wraps an integration event for publishing to a message broker (e.g. Kafka).
/// </summary>
/// <param name="EventId">Unique identifier for this specific event occurrence.</param>
/// <param name="EventType">
///   Short, dot-separated event type name used by consumers to route and deserialise
///   the payload (e.g. <c>"social.match.created"</c>).
/// </param>
/// <param name="OccurredAtUtc">UTC timestamp of when the originating domain event occurred.</param>
/// <param name="CorrelationId">
///   Optional correlation identifier for distributed tracing; propagated from the
///   inbound request or generated at the service boundary.
/// </param>
/// <param name="Payload">
///   Serialised event payload. Kept as a <see cref="JsonElement"/> so the envelope
///   layer remains agnostic of individual event schemas.
/// </param>
public sealed record IntegrationEventEnvelope(
    Guid EventId,
    string EventType,
    DateTimeOffset OccurredAtUtc,
    string? CorrelationId,
    JsonElement Payload);
