using Lunara.Moderation.Domain.Entities;

namespace Lunara.Moderation.Application.Ports;

/// <summary>Persistence port for storing <see cref="Report"/> entities.</summary>
public interface IReportRepository
{
    /// <summary>Persists a new <paramref name="report"/>.</summary>
    Task AddAsync(Report report, CancellationToken ct);
}
