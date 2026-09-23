using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using US.Domain.Entities;
using Monitor = US.Domain.Entities.Monitor;

namespace US.Infrastructure.Persistence.Configurations;

public sealed class MonitorConfiguration : IEntityTypeConfiguration<Monitor>
{
    public void Configure(EntityTypeBuilder<Monitor> builder)
    {
        builder.ToTable("monitors");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.Target)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(m => m.Type)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(m => m.IntervalSeconds)
            .IsRequired()
            .HasDefaultValue(60);

        builder.Property(m => m.TimeoutMs)
            .IsRequired()
            .HasDefaultValue(5000);

        builder.Property(m => m.AlertThreshold)
            .IsRequired()
            .HasDefaultValue(3);

        builder.Property(m => m.RecoveryThreshold)
            .IsRequired()
            .HasDefaultValue(3);

        builder.Property(m => m.NotifyOnRecovery)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(m => m.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(m => m.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(m => m.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Navigation(m => m.State)
            .IsRequired()
            .AutoInclude();

        builder.HasIndex(m => m.IsActive);
        
        builder.HasIndex(m => m.OrganizationId);

        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(m => m.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}