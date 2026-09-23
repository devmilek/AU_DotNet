using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using US.Domain.Entities;
using Monitor = US.Domain.Entities.Monitor;

namespace US.Infrastructure.Persistence.Configurations;

public class MonitorNotificationChannelConfiguration : IEntityTypeConfiguration<MonitorNotificationChannel>
{
    public void Configure(EntityTypeBuilder<MonitorNotificationChannel> builder)
    {
        builder.ToTable("monitor_notification_channels");
        
        builder.HasKey(x => new { x.MonitorId, x.NotificationChannelId });
        
        builder.HasOne<Monitor>().WithMany().HasForeignKey(x => x.MonitorId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<NotificationChannel>().WithMany().HasForeignKey(x => x.NotificationChannelId).OnDelete(DeleteBehavior.Cascade);
    }
}