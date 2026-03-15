using Lunara.Moderation.Application.Ports;
using Lunara.Moderation.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lunara.Moderation.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of <see cref="IReportRepository"/>.
/// </summary>
internal sealed class EfReportRepository(DbContext dbContext) : IReportRepository
{
    /// <inheritdoc/>
    public async Task AddAsync(Report report, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(report);

        ReportEntity entity = ToEntity(report);
        await dbContext.Set<ReportEntity>().AddAsync(entity, ct).ConfigureAwait(false);
    }

    // ── Mapping ────────────────────────────────────────────────────────────

    private static ReportEntity ToEntity(Report r)
    {
        return new ReportEntity
        {
            Id = r.Id.Value,
            ReporterUserId = r.ReporterUserId.Value,
            TargetUserId = r.TargetUserId.Value,
            Reason = r.Reason,
            Details = r.Details,
            CreatedAtUtc = r.CreatedAtUtc,
        };
    }
}
