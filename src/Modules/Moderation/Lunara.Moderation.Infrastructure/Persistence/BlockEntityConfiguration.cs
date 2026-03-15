using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lunara.Moderation.Infrastructure.Persistence;

/// <summary>EF Core configuration for the <see cref="BlockEntity"/>.</summary>
internal sealed class BlockEntityConfiguration : IEntityTypeConfiguration<BlockEntity>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<BlockEntity> builder)
    {
        builder.ToTable("moderation_blocks");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.BlockerUserId)
               .IsRequired();

        builder.Property(e => e.BlockedUserId)
               .IsRequired();

        builder.Property(e => e.CreatedAtUtc)
               .IsRequired();

        // Enforce uniqueness: a blocker can only block a given user once.
        builder.HasIndex(e => new { e.BlockerUserId, e.BlockedUserId })
               .IsUnique()
               .HasDatabaseName("ix_moderation_blocks_blocker_blocked");

        // Speed up bidirectional block checks.
        builder.HasIndex(e => e.BlockedUserId)
               .HasDatabaseName("ix_moderation_blocks_blocked");
    }
}
