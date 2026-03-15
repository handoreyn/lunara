using Lunara.Moderation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lunara.Moderation.Infrastructure.Persistence;

/// <summary>EF Core configuration for the <see cref="ReportEntity"/>.</summary>
internal sealed class ReportEntityConfiguration : IEntityTypeConfiguration<ReportEntity>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<ReportEntity> builder)
    {
        builder.ToTable("moderation_reports");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ReporterUserId)
               .IsRequired();

        builder.Property(e => e.TargetUserId)
               .IsRequired();

        builder.Property(e => e.Reason)
               .HasMaxLength(Report.MaxReasonLength)
               .IsRequired();

        builder.Property(e => e.Details)
               .HasMaxLength(Report.MaxReasonLength);

        builder.Property(e => e.CreatedAtUtc)
               .IsRequired();

        // Index for querying reports about a specific target user.
        builder.HasIndex(e => e.TargetUserId)
               .HasDatabaseName("ix_moderation_reports_target");
    }
}
