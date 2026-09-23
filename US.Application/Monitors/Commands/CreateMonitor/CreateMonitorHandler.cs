namespace US.Application.Monitors.Commands.CreateMonitor;
using Monitor = US.Domain.Entities.Monitor;

public sealed class CreateMonitorHandler
{
    public async Task<Guid> Handle(
        CreateMonitorCommand command,
        IMonitorRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var monitor = Monitor.Create(
            command.OrganizationId,
            command.Name,
            command.Target,
            command.Type,
            command.IntervalSeconds,
            command.TimeoutMs,
            command.AlertThreshold,
            command.RecoveryThreshold);

        await repository.AddAsync(monitor, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return monitor.Id;
    }
}
