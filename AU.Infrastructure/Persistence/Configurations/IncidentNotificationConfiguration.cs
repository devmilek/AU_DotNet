using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AU.Domain.Entities;

namespace AU.Infrastructure.Persistence.Configurations;

public sealed class IncidentNotificationConfiguration : IEntityTypeConfiguration<IncidentNotification>
{
    public void Configure(EntityTypeBuilder<IncidentNotification> builder)
    {
        builder.ToTable("incidents_notifications");

        builder.HasKey(n => n.Id);

        builder.HasIndex(n => n.IncidentId);

        builder.Property(n => n.Type).HasConversion<string>().IsRequired();
        builder.Property(n => n.Channel).HasConversion<string>().IsRequired();
        builder.Property(n => n.Status).HasConversion<string>().IsRequired();

        builder.Property(n => n.ErrorMessage).HasMaxLength(500);

        builder.HasOne<Incident>()
            .WithMany()
            .HasForeignKey(n => n.IncidentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}