using US.Application.Exceptions;

namespace US.Application.MaintenanceWindows.Commands.UpdateMaintenanceWindowSchedule;

public sealed record UpdateMaintenanceWindowScheduleCommand(
    Guid OrganizationId,
    Guid WindowId,
    MaintenanceScheduleInput Schedule);

public sealed class UpdateMaintenanceWindowScheduleHandler
{
    public async Task Handle(
        UpdateMaintenanceWindowScheduleCommand command,
        IMaintenanceWindowRepository windowRepository,
        IRecurrenceExpander expander,
        TimeProvider timeProvider,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();

        var window = await windowRepository.GetAsync(command.OrganizationId, command.WindowId, now, ct: cancellationToken)
                     ?? throw new NotFoundException("Okno serwisowe", command.WindowId);

        var schedule = command.Schedule;
        window.UpdateSchedule(
            schedule.TimeZoneId,
            schedule.StartsAtLocal,
            schedule.DurationMinutes,
            schedule.RecurrenceRule,
            schedule.RecurrenceEndLocal,
            now);

        var removed = MaintenanceSchedule.Sync(window, expander, now);
        windowRepository.RemoveOccurrences(removed);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
