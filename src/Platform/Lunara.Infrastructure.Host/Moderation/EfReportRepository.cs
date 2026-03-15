using Lunara.Infrastructure.Host.Persistence;
using Lunara.Moderation.Application.Ports;
using Lunara.Moderation.Domain.Entities;

namespace Lunara.Infrastructure.Host.Moderation;

/// <summary>EF Core–backed implementation of <see cref="IReportRepository"/>.</summary>
internal sealed class EfReportRepository(LunaraDbContext dbContext) : IReportRepository
{
    /// <inheritdoc/>
    public async Task AddAsync(Report report, CancellationToken ct)
    {
        ReportEntity entity = new()
        {
            Id = report.Id.Value,
            ReporterUserId = report.ReporterUserId.Value,
            TargetUserId = report.TargetUserId.Value,
            Reason = report.Reason,
            Details = report.Details,
            CreatedAtUtc = report.CreatedAtUtc,
        };

        await dbContext.Reports.AddAsync(entity, ct).ConfigureAwait(false);
    }
}
