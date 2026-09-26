using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AU.Domain.Entities;
using Monitor = AU.Domain.Entities.Monitor;

namespace AU.Infrastructure.Persistence.Configurations;

public sealed class MaintenanceWindowMonitorConfiguration : IEntityTypeConfiguration<MaintenanceWindowMonitor>
{
    public void Configure(EntityTypeBuilder<MaintenanceWindowMonitor> builder)
    {
        builder.ToTable("maintenance_window_monitors");

        builder.HasKey(x => new { x.MaintenanceWindowId, x.MonitorId });

        builder.HasIndex(x => x.MonitorId);

        builder.HasOne<Monitor>()
            .WithMany()
            .HasForeignKey(x => x.MonitorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
