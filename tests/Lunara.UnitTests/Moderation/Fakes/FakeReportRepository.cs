using Lunara.Moderation.Application.Ports;
using Lunara.Moderation.Domain.Entities;

namespace Lunara.UnitTests.Moderation.Fakes;

/// <summary>In-memory fake for <see cref="IReportRepository"/> used in unit tests.</summary>
public sealed class FakeReportRepository : IReportRepository
{
    private readonly List<Report> _reports = [];

    /// <summary>Gets all reports that were passed to <see cref="AddAsync"/>.</summary>
    public IReadOnlyList<Report> AddedReports => _reports;

    /// <inheritdoc/>
    public Task AddAsync(Report report, CancellationToken ct)
    {
        _reports.Add(report);
        return Task.CompletedTask;
    }
}
