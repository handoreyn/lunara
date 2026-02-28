using Lunara.Social.Domain.ValueObjects;

namespace Lunara.Social.Application.Ports;

/// <summary>Persistence port for recording and querying likes between users.</summary>
public interface ILikeRepository
{
    /// <summary>Returns <c>true</c> if <paramref name="from"/> has already liked <paramref name="recipient"/>.</summary>
    Task<bool> ExistsLikeAsync(UserId from, UserId recipient, CancellationToken ct);

    /// <summary>Persists a new like from <paramref name="from"/> to <paramref name="recipient"/>.</summary>
    Task AddLikeAsync(UserId from, UserId recipient, DateTimeOffset atUtc, CancellationToken ct);
}
