using Lunara.Moderation.Domain.Entities;

namespace Lunara.Moderation.Application.Ports;

/// <summary>Persistence port for <see cref="Report"/> aggregates.</summary>
public interface IReportRepository
{
    /// <summary>
    /// Adds a new report to the store.
    /// The change is not persisted until <see cref="IUnitOfWork.SaveChangesAsync"/> is called.
    /// </summary>
    /// <param name="report">The report to add.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    Task AddAsync(Report report, CancellationToken ct);
}
