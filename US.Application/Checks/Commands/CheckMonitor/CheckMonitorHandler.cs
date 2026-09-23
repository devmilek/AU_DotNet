using US.Application.Incidents.Events.MonitorCheckCompleted;
using US.Application.Monitors;
using US.Domain.Entities;
using US.Domain.Enums;
using Wolverine;

namespace US.Application.Checks.Commands.CheckMonitor;

public class CheckMonitorHandler
{
    public async Task Handle(CheckMonitorCommand command, IMonitorRepository monitorRepository, IChecksRepository checksRepository, IEnumerable<IMonitorChecker> checkers, IUnitOfWork unitOfWork, IMessageBus bus)
    {
        var monitor = await monitorRepository.GetForCheckAsync(command.MonitorId);
        if (monitor is null)
        {
            throw new InvalidOperationException($"Monitor with ID {command.MonitorId} not found.");
        }
        
        var checker = checkers.First(c => c.Type == monitor.Type);
        
        var result = await checker.CheckAsync(monitor);

        await checksRepository.AddAsync(result);
        await unitOfWork.SaveChangesAsync();
        
        var isUp = result.Status == CheckStatus.UP;
        await bus.PublishAsync(new MonitorCheckCompletedEvent(monitor.Id, isUp, result.CheckedAt));
    }
}