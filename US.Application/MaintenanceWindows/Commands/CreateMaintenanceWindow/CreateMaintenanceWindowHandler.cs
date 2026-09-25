using US.Application.Abstractions;
using US.Application.Channels;
using US.Application.Monitors;
using US.Domain.Entities;

namespace US.Application.MaintenanceWindows.Commands.CreateMaintenanceWindow;

public sealed class CreateMaintenanceWindowHandler
{
    public async Task<Guid> Handle(
        CreateMaintenanceWindowCommand command,
        IMaintenanceWindowRepository windowRepository,
        IMonitorRepository monitorRepository,
        IRecurrenceExpander expander,
        ICurrentUser currentUser,
        TimeProvider timeProvider,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var monitorIds = await MonitorAssignment.EnsureMonitorsExistAsync(
            monitorRepository, command.OrganizationId, command.MonitorIds, cancellationToken);

        var now = timeProvider.GetUtcNow();
        var schedule = command.Schedule;

        var window = MaintenanceWindow.Create(
            command.OrganizationId,
            currentUser.UserId,
            command.Name,
            command.Description,
            schedule.TimeZoneId,
            schedule.StartsAtLocal,
            schedule.DurationMinutes,
            monitorIds,
            schedule.RecurrenceRule,
            schedule.RecurrenceEndLocal,
            command.SuppressNotifications,
            command.ExcludeFromSla,
            now);

        MaintenanceSchedule.Sync(window, expander, now);

        windowRepository.Add(window);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return window.Id;
    }
}
