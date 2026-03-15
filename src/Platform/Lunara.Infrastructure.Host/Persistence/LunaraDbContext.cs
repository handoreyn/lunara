using Lunara.Infrastructure.Host.Moderation;
using Microsoft.EntityFrameworkCore;

namespace Lunara.Infrastructure.Host.Persistence;

/// <summary>
/// EF Core database context for all Lunara platform tables.
/// </summary>
internal sealed class LunaraDbContext(DbContextOptions<LunaraDbContext> options) : DbContext(options)
{
    /// <summary>Gets the outbox messages table used for the Transactional Outbox pattern.</summary>
    public DbSet<OutboxMessageEntity> OutboxMessages { get; set; } = null!;

    /// <summary>Gets the moderation blocks table.</summary>
    public DbSet<BlockEntity> Blocks { get; set; } = null!;

    /// <summary>Gets the moderation reports table.</summary>
    public DbSet<ReportEntity> Reports { get; set; } = null!;

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

        modelBuilder.Entity<BlockEntity>(entity =>
        {
            entity.ToTable("moderation_blocks");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BlockerUserId).HasColumnName("blocker_user_id").IsRequired();
            entity.Property(e => e.BlockedUserId).HasColumnName("blocked_user_id").IsRequired();
            entity.Property(e => e.CreatedAtUtc).HasColumnName("created_at_utc");

            // Unique index ensures one block record per ordered pair.
            entity.HasIndex(e => new { e.BlockerUserId, e.BlockedUserId })
                  .IsUnique()
                  .HasDatabaseName("ix_moderation_blocks_pair");
        });

        modelBuilder.Entity<ReportEntity>(entity =>
        {
            entity.ToTable("moderation_reports");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ReporterUserId).HasColumnName("reporter_user_id").IsRequired();
            entity.Property(e => e.TargetUserId).HasColumnName("target_user_id").IsRequired();
            entity.Property(e => e.Reason).HasColumnName("reason").HasMaxLength(500).IsRequired();
            entity.Property(e => e.Details).HasColumnName("details");
            entity.Property(e => e.CreatedAtUtc).HasColumnName("created_at_utc");
        });
    }
}
