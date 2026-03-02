using Lunara.Messaging.Domain.ValueObjects;

namespace Lunara.Messaging.Application.Ports;

/// <summary>
/// Holds the canonical participant pair returned by <see cref="IMatchReadService.GetParticipantsAsync"/>.
/// <para>
/// Participants are in the same canonical order used by the Social module:
/// <see cref="User1Id"/> is the lexicographically smaller <see cref="Guid"/>,
/// <see cref="User2Id"/> is the larger.
/// </para>
/// </summary>
/// <param name="User1Id">The participant with the lexicographically smaller identifier.</param>
/// <param name="User2Id">The participant with the lexicographically larger identifier.</param>
public sealed record MatchParticipants(UserId User1Id, UserId User2Id);
