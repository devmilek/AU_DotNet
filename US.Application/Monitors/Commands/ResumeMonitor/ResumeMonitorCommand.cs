using US.Application.Exceptions;

namespace US.Application.Monitors.Commands.ResumeMonitor;

public sealed record ResumeMonitorCommand(Guid OrganizationId, Guid MonitorId);

public sealed class ResumeMonitorHandler
{
    public async Task Handle(
        ResumeMonitorCommand command,
        IMonitorRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var monitor = await repository.GetAsync(command.OrganizationId, command.MonitorId, cancellationToken)
                      ?? throw new NotFoundException("Monitor", command.MonitorId);

        // NextCheckAt z czasu pauzy jest już w przeszłości, więc scheduler sprawdzi monitor przy najbliższym ticku
        monitor.Activate();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
