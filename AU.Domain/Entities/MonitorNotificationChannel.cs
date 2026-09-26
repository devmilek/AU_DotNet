namespace AU.Domain.Entities;

public class MonitorNotificationChannel
{
    public Guid MonitorId { get; private set; }
    public Guid NotificationChannelId { get; private set; }
    
    private MonitorNotificationChannel() { }

    public MonitorNotificationChannel(Guid monitorId, Guid notificationChannelId)
    {
        MonitorId = monitorId;
        NotificationChannelId = notificationChannelId;
    }
}