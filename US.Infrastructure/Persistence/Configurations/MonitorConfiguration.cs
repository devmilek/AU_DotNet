using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using US.Domain.Entities;
using US.Domain.ValueObjects.Checks;
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

        builder.Property(m => m.Config)
            .HasColumnType("jsonb")
            .HasConversion(
                config => config == null ? null : JsonSerializer.Serialize(config, JsonOptions),
                json => json == null ? null : JsonSerializer.Deserialize<CheckConfig>(json, JsonOptions))
            .Metadata.SetValueComparer(new ValueComparer<CheckConfig?>(
                (a, b) => JsonSerializer.Serialize(a, JsonOptions) == JsonSerializer.Serialize(b, JsonOptions),
                c => JsonSerializer.Serialize(c, JsonOptions).GetHashCode(),
                c => c == null ? null : JsonSerializer.Deserialize<CheckConfig>(JsonSerializer.Serialize(c, JsonOptions), JsonOptions)));

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
            .IsRequired();

        builder.HasIndex(m => m.IsActive);
        
        builder.HasIndex(m => m.OrganizationId);

        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(m => m.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static JsonSerializerOptions JsonOptions => JsonColumnOptions.Default;
}