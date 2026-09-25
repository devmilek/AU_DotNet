using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using US.Domain.Entities;

namespace US.Infrastructure.Persistence.Configurations;

public sealed class MaintenanceWindowConfiguration : IEntityTypeConfiguration<MaintenanceWindow>
{
    public void Configure(EntityTypeBuilder<MaintenanceWindow> builder)
    {
        builder.ToTable("maintenance_windows");

        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id).ValueGeneratedNever();

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(MaintenanceWindow.MaxNameLength);

        builder.Property(w => w.Description)
            .HasMaxLength(MaintenanceWindow.MaxDescriptionLength);

        builder.Property(w => w.TimeZoneId)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(w => w.StartsAtLocal)
            .IsRequired()
            .HasColumnType("timestamp without time zone");

        builder.Property(w => w.RecurrenceEndLocal)
            .HasColumnType("timestamp without time zone");

        builder.Property(w => w.RecurrenceRule)
            .HasMaxLength(MaintenanceWindow.MaxRecurrenceRuleLength);

        builder.Property(w => w.DurationMinutes).IsRequired();
        builder.Property(w => w.SuppressNotifications).IsRequired();
        builder.Property(w => w.ExcludeFromSla).IsRequired();

        builder.Property(w => w.CreatedAt).IsRequired();
        builder.Property(w => w.UpdatedAt).IsRequired();

        builder.Ignore(w => w.IsRecurring);
        builder.Ignore(w => w.IsDeleted);
        builder.Ignore(w => w.Duration);

        builder.HasIndex(w => w.OrganizationId)
            .HasFilter("\"DeletedAt\" IS NULL");

        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(w => w.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(w => w.CreatedByUserId).IsRequired();

        builder.HasMany(w => w.Monitors)
            .WithOne()
            .HasForeignKey(m => m.MaintenanceWindowId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(w => w.Monitors).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(w => w.Occurrences)
            .WithOne()
            .HasForeignKey(o => o.MaintenanceWindowId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Navigation(w => w.Occurrences).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
