using Lunara.Messaging.Application.Ports;

namespace Lunara.UnitTests.Modules.Messaging.Fakes;

/// <summary>
/// In-memory fake for <see cref="IUnitOfWork"/> used in Messaging unit tests.
/// </summary>
public sealed class FakeUnitOfWork : IUnitOfWork
{
    /// <summary>Gets the number of times <see cref="SaveChangesAsync"/> was called.</summary>
    public int SaveCallCount { get; private set; }

    /// <summary>
    /// When set, the next call to <see cref="SaveChangesAsync"/> throws this exception
    /// instead of completing. The property is cleared after firing so that subsequent
    /// calls succeed normally.
    /// </summary>
    public Exception? ThrowOnFirstSave { get; set; }

    /// <summary>
    /// Optional side-effect invoked immediately before the exception set in
    /// <see cref="ThrowOnFirstSave"/> is thrown, simulating changes that become
    /// visible to other callers after a failed save (e.g. the concurrent winner
    /// becoming visible in the repository).
    /// </summary>
    public Action? AfterThrow { get; set; }

    /// <inheritdoc/>
    public Task SaveChangesAsync(CancellationToken ct)
    {
        SaveCallCount++;
        if (ThrowOnFirstSave is not null)
        {
            Exception toThrow = ThrowOnFirstSave;
            ThrowOnFirstSave = null;
            AfterThrow?.Invoke();
            AfterThrow = null;
            throw toThrow;
        }

        return Task.CompletedTask;
    }
}
