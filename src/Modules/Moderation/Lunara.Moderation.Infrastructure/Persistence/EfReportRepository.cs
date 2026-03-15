using Lunara.Moderation.Application.Ports;
using Lunara.Moderation.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lunara.Moderation.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of <see cref="IReportRepository"/>.
/// </summary>
internal sealed class EfReportRepository(DbContext context) : IReportRepository
{
    private DbSet<ReportEntity> Reports => context.Set<ReportEntity>();

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

        await Reports.AddAsync(entity, ct).ConfigureAwait(false);
    }
}
