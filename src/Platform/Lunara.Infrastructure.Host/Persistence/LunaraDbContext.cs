using Lunara.Moderation.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Lunara.Infrastructure.Host.Persistence;

/// <summary>
/// EF Core database context for all Lunara platform tables.
/// </summary>
internal sealed class LunaraDbContext(DbContextOptions<LunaraDbContext> options) : DbContext(options)
{
    /// <summary>Gets the outbox messages table used for the Transactional Outbox pattern.</summary>
    public DbSet<OutboxMessageEntity> OutboxMessages { get; set; } = null!;

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OutboxMessageEntity>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.EventType)
                  .HasMaxLength(256)
                  .IsRequired();

            entity.Property(e => e.PayloadJson)
                  .IsRequired();

            entity.Property(e => e.LastError)
                  .HasMaxLength(2000);

            // Composite index for the outbox poller: fetch pending/failed rows ordered by time.
            entity.HasIndex(e => new { e.Status, e.OccurredAtUtc })
                  .HasDatabaseName("ix_outbox_status_occurred");

            // Index for the lock-expiry sweep.
            entity.HasIndex(e => e.LockedUntilUtc)
                  .HasDatabaseName("ix_outbox_locked_until");
        });

        // Apply all entity configurations defined in Lunara.Moderation.Infrastructure.
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ModerationInfrastructureAssemblyMarker).Assembly);
    }
}
