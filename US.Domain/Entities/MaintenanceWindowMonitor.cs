namespace US.Domain.Entities;

public class MaintenanceWindowMonitor
{
    public Guid MaintenanceWindowId { get; private set; }
    public Guid MonitorId { get; private set; }

    private MaintenanceWindowMonitor() { }

    internal MaintenanceWindowMonitor(Guid monitorId, Guid maintenanceWindowId)
    {
        MonitorId = monitorId;
        MaintenanceWindowId = maintenanceWindowId;
    }
}
