using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using US.Domain.Entities;
using US.Domain.ValueObjects;
using US.Infrastructure.Persistence;

namespace US.Infrastructure.Persistence.Configurations;

public class NotificationChannelConfiguration : IEntityTypeConfiguration<NotificationChannel>
{
    public void Configure(EntityTypeBuilder<NotificationChannel> builder)
    {
        builder.ToTable("notification_channels");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(c => c.Type)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(c => c.Config)
            .HasConversion(
                config => JsonSerializer.Serialize(config, JsonOptions),
                json => JsonSerializer.Deserialize<ChannelConfig>(json, JsonOptions)!
            ).HasColumnType("jsonb")
            .Metadata.SetValueComparer(new ValueComparer<ChannelConfig>(
                (a, b) => JsonSerializer.Serialize(a, JsonOptions) == JsonSerializer.Serialize(b, JsonOptions),
                c => JsonSerializer.Serialize(c, JsonOptions).GetHashCode(),
                c => JsonSerializer.Deserialize<ChannelConfig>(JsonSerializer.Serialize(c, JsonOptions),
                    JsonOptions)!));
        
        builder.HasIndex(m => m.OrganizationId);

        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(m => m.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
    
    private static JsonSerializerOptions JsonOptions => JsonColumnOptions.Default;
}