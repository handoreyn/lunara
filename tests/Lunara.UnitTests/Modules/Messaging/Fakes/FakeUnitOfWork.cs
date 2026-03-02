using Lunara.Messaging.Application.Ports;

namespace Lunara.UnitTests.Modules.Messaging.Fakes;

/// <summary>
/// In-memory fake for <see cref="IUnitOfWork"/> used in Messaging unit tests.
/// </summary>
public sealed class FakeUnitOfWork : IUnitOfWork
{
    /// <summary>Gets the number of times <see cref="SaveChangesAsync"/> was called.</summary>
    public int SaveCallCount { get; private set; }

    /// <inheritdoc/>
    public Task SaveChangesAsync(CancellationToken ct)
    {
        SaveCallCount++;
        return Task.CompletedTask;
    }
}
