using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AU.Domain.Entities;

namespace AU.Infrastructure.Persistence.Configurations;

public sealed class MaintenanceOccurrenceConfiguration : IEntityTypeConfiguration<MaintenanceOccurrence>
{
    public void Configure(EntityTypeBuilder<MaintenanceOccurrence> builder)
    {
        builder.ToTable("maintenance_occurrences");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).ValueGeneratedNever();

        builder.Property(o => o.Name)
            .IsRequired()
            .HasMaxLength(MaintenanceWindow.MaxNameLength);

        builder.Property(o => o.Description)
            .HasMaxLength(MaintenanceWindow.MaxDescriptionLength);

        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(o => o.StartsAtUtc).IsRequired();
        builder.Property(o => o.EndsAtUtc).IsRequired();
        builder.Property(o => o.ScheduledStartUtc).IsRequired();
        builder.Property(o => o.CreatedAt).IsRequired();
        builder.Property(o => o.UpdatedAt).IsRequired();

        builder.Ignore(o => o.IsCancelled);
        builder.Ignore(o => o.IsRescheduled);
        builder.Ignore(o => o.IsContentLocked);

        builder.HasIndex(o => new { o.MaintenanceWindowId, o.ScheduledStartUtc })
            .IsUnique();

        builder.HasIndex(o => new { o.StartsAtUtc, o.EndsAtUtc })
            .HasFilter("\"Status\" = 'Scheduled'");
    }
}
