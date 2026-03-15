using Lunara.Moderation.Application.Ports;
using Lunara.Moderation.Domain.Entities;

namespace Lunara.UnitTests.Modules.Moderation.Fakes;

/// <summary>
/// In-memory fake for <see cref="IReportRepository"/> used in Moderation unit tests.
/// </summary>
public sealed class FakeReportRepository : IReportRepository
{
    private readonly List<Report> _reports = [];

    /// <summary>Gets all reports that were added via <see cref="AddAsync"/>.</summary>
    public IReadOnlyList<Report> AddedReports => _reports.AsReadOnly();

    /// <summary>Gets the number of times <see cref="AddAsync"/> was called.</summary>
    public int AddCallCount { get; private set; }

    /// <inheritdoc/>
    public Task AddAsync(Report report, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(report);
        AddCallCount++;
        _reports.Add(report);
        return Task.CompletedTask;
    }
}
