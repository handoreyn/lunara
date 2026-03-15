using Lunara.Moderation.Application.DTOs;
using Lunara.Moderation.Application.Ports;
using Lunara.Moderation.Domain.Entities;

namespace Lunara.Moderation.Application.UseCases;

/// <summary>
/// Handles filing a report against a user.
/// </summary>
public sealed class ReportUserService(
    IReportRepository reportRepository,
    IUnitOfWork unitOfWork,
    IClock clock)
{
    /// <summary>
    /// Files a report on behalf of <paramref name="request"/>.<c>ReporterUserId</c>
    /// against <paramref name="request"/>.<c>TargetUserId</c>.
    /// </summary>
    /// <param name="request">The report details.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A <see cref="ReportUserResult"/> with the new report identifier.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    ///   Thrown when reporter and target are the same user, or when the reason is invalid.
    /// </exception>
    public async Task<ReportUserResult> ReportAsync(
        ReportUserRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        Report report = Report.Create(
            request.ReporterUserId,
            request.TargetUserId,
            request.Reason,
            request.Details,
            clock.UtcNow);

        await reportRepository.AddAsync(report, ct).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return new ReportUserResult(report.Id.Value);
    }
}
