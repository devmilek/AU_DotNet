using AU.Application.Abstractions;

namespace AU.Application.Monitors.Commands.CreateMonitor;
using Monitor = AU.Domain.Entities.Monitor;

public sealed class CreateMonitorHandler
{
    public async Task<Guid> Handle(
        CreateMonitorCommand command,
        IMonitorRepository repository,
        ISecretProtector secretProtector,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var monitor = Monitor.Create(
            command.OrganizationId,
            command.Name,
            command.Target,
            command.Type,
            command.Http is { } http ? HttpCheckSettingsMapper.ToConfig(http, secretProtector) : null,
            command.IntervalSeconds,
            command.TimeoutMs,
            command.AlertThreshold,
            command.RecoveryThreshold);

        await repository.AddAsync(monitor, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return monitor.Id;
    }
}
