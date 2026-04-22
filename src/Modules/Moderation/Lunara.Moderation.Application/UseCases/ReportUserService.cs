using Lunara.BuildingBlocks.Clocks;
using Lunara.Moderation.Application.DTOs;
using Lunara.Moderation.Application.Ports;
using Lunara.Moderation.Domain.Entities;
using Lunara.Moderation.Domain.ValueObjects;

namespace Lunara.Moderation.Application.UseCases;

/// <summary>
/// Files a report against a user on behalf of another user.
/// </summary>
public sealed class ReportUserService(
    IReportRepository reportRepository,
    IUnitOfWork unitOfWork,
    IClock clock)
{
    /// <summary>
    /// Creates a report filed by <see cref="ReportUserRequest.ReporterUserId"/> against
    /// <see cref="ReportUserRequest.TargetUserId"/>.
    /// </summary>
    /// <param name="request">The report request.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A <see cref="ReportUserResult"/> containing the identifier of the created report.</returns>
    /// <exception cref="ArgumentNullException">
    ///   Thrown when <paramref name="request"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   Thrown when validation on the request fails (self-report, empty reason, reason too long).
    /// </exception>
    public async Task<ReportUserResult> ReportAsync(ReportUserRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        UserId reporterUserId = new(request.ReporterUserId);
        UserId targetUserId = new(request.TargetUserId);

        Report report = Report.Create(
            reporterUserId,
            targetUserId,
            request.Reason,
            request.Details,
            clock.UtcNow);

        await reportRepository.AddAsync(report, ct).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return new ReportUserResult(ReportId: report.Id.Value);
    }
}
