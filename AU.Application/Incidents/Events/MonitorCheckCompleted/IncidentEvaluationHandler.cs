using AU.Application.Incidents.Events.IncidentOpened;
using AU.Application.Monitors;
using AU.Domain.Entities;
using Wolverine;

namespace AU.Application.Incidents.Events.MonitorCheckCompleted;

public class IncidentEvaluationHandler
{
    public async Task Handle(MonitorCheckCompletedEvent @event, IMonitorRepository monitorRepository, IIncidentRepository incidentRepository, IUnitOfWork unitOfWork, IMessageBus bus)
    {
        var monitor = await monitorRepository.GetForCheckAsync(@event.MonitorId);
        if (monitor is null) return;
        
        Guid? openedIncidentId = null;
        Guid? resolvedIncidentId = null;

        if (!@event.IsUp)
        {
            var reachedThreshold = monitor.RecordFailure();
            var openIncident = await incidentRepository.GetOpenForMonitorAsync(monitor.Id);

            if (reachedThreshold && openIncident is null)
            {
                var incident = Incident.Open(monitor.Id, @event.CheckedAt, monitor.State.ConsecutiveFailures);
                incidentRepository.Add(incident);
                openedIncidentId = incident.Id;
            }
            else if (openIncident is not null)
            {
                // incydent juz trwa wiec doliczamy tylko failed check
                openIncident.IncrementFailedChecks();
            }
        }
        else
        {
            var shouldResolve = monitor.RecordSuccess();

            if (shouldResolve)
            {
                var openIncident = await incidentRepository.GetOpenForMonitorAsync(monitor.Id);
                if (openIncident is not null)
                {
                    openIncident.Resolve(@event.CheckedAt);
                    resolvedIncidentId = openIncident.Id;
                }
            }
        }

        await unitOfWork.SaveChangesAsync();
        
        if (openedIncidentId is { } incidentId)
            await bus.PublishAsync(new IncidentOpenedEvent(incidentId, monitor.Id, @event.CheckedAt));

        //if (resolvedIncidentId is { } resolvedId)
          //  await bus.PublishAsync(new IncidentResolvedEvent(resolvedId, monitor.Id, @event.CheckedAt));
    }
}