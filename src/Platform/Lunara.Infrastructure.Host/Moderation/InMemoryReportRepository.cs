using System.Collections.Concurrent;
using Lunara.Moderation.Application.Ports;
using Lunara.Moderation.Domain.Entities;

namespace Lunara.Infrastructure.Host.Moderation;

/// <summary>
/// Thread-safe in-memory implementation of <see cref="IReportRepository"/> for local development.
/// Data is not persisted across application restarts.
/// </summary>
internal sealed class InMemoryReportRepository : IReportRepository
{
    private readonly ConcurrentBag<Report> _reports = [];

    /// <inheritdoc/>
    public Task AddAsync(Report report, CancellationToken ct)
    {
        _reports.Add(report);
        return Task.CompletedTask;
    }
}
