using Lunara.Social.Domain.Entities;

namespace Lunara.Social.Application.Ports;

/// <summary>Persistence port for storing matches.</summary>
public interface IMatchRepository
{
    /// <summary>Persists a newly created <paramref name="match"/>.</summary>
    Task AddAsync(Match match, CancellationToken ct);
}
