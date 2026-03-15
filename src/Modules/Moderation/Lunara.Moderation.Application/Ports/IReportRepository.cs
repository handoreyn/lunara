namespace Lunara.Moderation.Application.Ports;

/// <summary>Persistence port for storing and querying user reports.</summary>
public interface IReportRepository
{
    /// <summary>Persists a new report.</summary>
    Task AddAsync(Domain.Entities.Report report, CancellationToken ct);
}
