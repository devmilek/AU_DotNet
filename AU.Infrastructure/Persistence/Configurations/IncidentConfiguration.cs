using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AU.Domain.Entities;
using Monitor = AU.Domain.Entities.Monitor;
using Incident = AU.Domain.Entities.Incident;

namespace AU.Infrastructure.Persistence.Configurations;

public sealed class IncidentConfiguration : IEntityTypeConfiguration<Incident>
{
    public void Configure(EntityTypeBuilder<Incident> builder)
    {
        builder.ToTable("incidents");

        builder.HasKey(i => i.Id);

        builder.HasIndex(i => new { i.MonitorId, i.Status });

        builder.Property(i => i.Status).HasConversion<string>().IsRequired();
        builder.Property(i => i.StartedAt).IsRequired();
        builder.Property(i => i.FailedChecksCount).IsRequired();

        builder.Property(i => i.StartedInMaintenance)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(i => i.CreatedAt).IsRequired();
        builder.Property(i => i.UpdatedAt).IsRequired();

        builder.HasOne<Monitor>()
            .WithMany()
            .HasForeignKey(i => i.MonitorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}