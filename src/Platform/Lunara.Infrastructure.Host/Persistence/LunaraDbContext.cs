using Lunara.Messaging.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lunara.Infrastructure.Host.Persistence;

/// <summary>
/// EF Core database context for all Lunara platform tables.
/// </summary>
internal sealed class LunaraDbContext(DbContextOptions<LunaraDbContext> options) : DbContext(options)
{
    /// <summary>Gets the outbox messages table used for the Transactional Outbox pattern.</summary>
    public DbSet<OutboxMessageEntity> OutboxMessages { get; set; } = null!;

    /// <summary>Gets the messaging conversations table.</summary>
    public DbSet<ConversationEntity> Conversations { get; set; } = null!;

    /// <summary>Gets the messaging messages table.</summary>
    public DbSet<MessageEntity> Messages { get; set; } = null!;

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

        modelBuilder.Entity<ConversationEntity>(entity =>
        {
            entity.ToTable("messaging_conversations");
            entity.HasKey(e => e.Id);

            // 1 match = 1 conversation enforced at the database level.
            entity.HasIndex(e => e.MatchId)
                  .IsUnique()
                  .HasDatabaseName("ix_messaging_conversations_match_id");
        });

        modelBuilder.Entity<MessageEntity>(entity =>
        {
            entity.ToTable("messaging_messages");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Text)
                  .HasMaxLength(2000)
                  .IsRequired();

            entity.HasIndex(e => e.ConversationId)
                  .HasDatabaseName("ix_messaging_messages_conversation_id");

            entity.HasOne(e => e.Conversation)
                  .WithMany()
                  .HasForeignKey(e => e.ConversationId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

