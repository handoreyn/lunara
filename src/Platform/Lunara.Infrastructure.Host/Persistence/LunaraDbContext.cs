using Lunara.Messaging.Infrastructure.Persistence;
using Lunara.Notifications.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lunara.Infrastructure.Host.Persistence;

/// <summary>
/// EF Core database context for all Lunara platform tables.
/// </summary>
internal sealed class LunaraDbContext(DbContextOptions<LunaraDbContext> options) : DbContext(options)
{
    /// <summary>Gets the outbox messages table used for the Transactional Outbox pattern.</summary>
    public DbSet<OutboxMessageEntity> OutboxMessages { get; set; } = null!;

    /// <summary>Gets the inbox messages table used for the Inbox idempotency pattern.</summary>
    public DbSet<InboxMessageEntity> InboxMessages { get; set; } = null!;

    /// <summary>Gets the notifications table.</summary>
    public DbSet<NotificationEntity> Notifications { get; set; } = null!;

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

        modelBuilder.Entity<InboxMessageEntity>(entity =>
        {
            entity.ToTable("inbox_messages");

            // Composite primary key — one row per (consumer, event).
            entity.HasKey(e => new { e.Consumer, e.EventId });

            entity.Property(e => e.Consumer)
                  .HasMaxLength(100)
                  .IsRequired();

            // Unique index enforces the idempotency gate at the database level.
            entity.HasIndex(e => new { e.Consumer, e.EventId })
                  .IsUnique()
                  .HasDatabaseName("ix_inbox_consumer_event_id");

            // Index to support querying unprocessed or recently processed messages.
            entity.HasIndex(e => e.ProcessedAtUtc)
                  .HasDatabaseName("ix_inbox_processed_at");
        });

        modelBuilder.Entity<NotificationEntity>(entity =>
        {
            entity.ToTable("notifications_notifications");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.PayloadJson)
                  .IsRequired();

            // Composite index: fetch all notifications for a user ordered by newest first.
            entity.HasIndex(e => new { e.UserId, e.CreatedAtUtc })
                  .IsDescending(false, true)
                  .HasDatabaseName("ix_notifications_user_id_created_at");
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

