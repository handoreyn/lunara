using Lunara.BuildingBlocks.Clocks;
using Lunara.Moderation.Application.DTOs;
using Lunara.Moderation.Application.Ports;
using Lunara.Moderation.Domain.Entities;

namespace Lunara.Moderation.Application.UseCases;

/// <summary>
/// Records a report submitted by one user about another.
/// </summary>
public sealed class ReportUserService(
    IReportRepository reportRepository,
    IUnitOfWork unitOfWork,
    IClock clock)
{
    /// <summary>
    /// Processes a report request and returns the outcome.
    /// </summary>
    /// <param name="request">The report details.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A <see cref="ReportUserResult"/> indicating the report was persisted.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    ///   Thrown when the request violates domain invariants (self-report, empty or oversized reason).
    /// </exception>
    public async Task<ReportUserResult> ReportAsync(ReportUserRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Domain factory validates all invariants.
        Report report = Report.Create(
            request.ReporterUserId,
            request.TargetUserId,
            request.Reason,
            request.Details,
            clock.UtcNow);

        await reportRepository.AddAsync(report, ct).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return new ReportUserResult(ReportCreated: true);
    }
}
