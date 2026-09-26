using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AU.Domain.Entities;
using Monitor = AU.Domain.Entities.Monitor;

namespace AU.Infrastructure.Persistence.Configurations;

public sealed class MonitorStateConfiguration : IEntityTypeConfiguration<MonitorState>
{
    public void Configure(EntityTypeBuilder<MonitorState> builder)
    {
        builder.ToTable("monitor_states");

        builder.HasKey(s => s.MonitorId);

        builder.Property(s => s.ConsecutiveSuccesses)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.ConsecutiveFailures)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.NextCheckAt)
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.HasIndex(s => s.NextCheckAt);

        builder.HasOne<Monitor>()
            .WithOne(m => m.State)
            .HasForeignKey<MonitorState>(s => s.MonitorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
