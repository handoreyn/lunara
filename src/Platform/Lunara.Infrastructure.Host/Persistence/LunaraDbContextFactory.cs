using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Lunara.Infrastructure.Host.Persistence;

/// <summary>
/// Design-time factory used by EF Core CLI tools (<c>dotnet ef migrations add</c>,
/// <c>dotnet ef database update</c>, etc.) when no application host is available.
/// The connection string here is for local development only; CI and production
/// use the value injected via <c>DatabaseOptions</c>.
/// </summary>
internal sealed class LunaraDbContextFactory : IDesignTimeDbContextFactory<LunaraDbContext>
{
    /// <inheritdoc/>
    public LunaraDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<LunaraDbContext> builder = new();

        builder.UseNpgsql(
            "Host=localhost;Port=5432;Database=lunara;Username=lunara;Password=lunara",
            npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history"));

        return new LunaraDbContext(builder.Options);
    }
}
