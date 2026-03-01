using Microsoft.Extensions.Logging;

namespace Lunara.Infrastructure.Host.Persistence;

/// <summary>
/// EF Core implementation of <see cref="IDatabaseReadinessChecker"/>.
/// Attempts a lightweight connection test; does not retry so the check is fast.
/// </summary>
internal sealed partial class DatabaseReadinessChecker(
    LunaraDbContext dbContext,
    ILogger<DatabaseReadinessChecker> logger) : IDatabaseReadinessChecker
{
    /// <inheritdoc/>
    public async Task<bool> IsReadyAsync(CancellationToken ct)
    {
        try
        {
            // CanConnectAsync opens and closes a connection without executing SQL.
            // It handles connectivity failures internally and returns false for them;
            // other exceptions (misconfiguration, etc.) are caught below.
            bool canConnect = await dbContext.Database
                .CanConnectAsync(ct)
                .ConfigureAwait(false);

            if (!canConnect)
            {
                LogDatabaseNotReady(logger);
            }

            return canConnect;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            LogReadinessCheckFailed(logger, ex.Message, ex);
            return false;
        }
    }

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "Database readiness check: CanConnectAsync returned false.")]
    private static partial void LogDatabaseNotReady(ILogger logger);

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "Database readiness check threw an unexpected exception: {Message}")]
    private static partial void LogReadinessCheckFailed(ILogger logger, string message, Exception ex);
}
