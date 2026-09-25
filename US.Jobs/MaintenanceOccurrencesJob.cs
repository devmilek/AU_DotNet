using Hangfire;
using US.Application.MaintenanceWindows.Jobs;

namespace US.Jobs;

public sealed class MaintenanceOccurrencesJob(MaintenanceOccurrenceExtender extender)
{
    public const string Id = "maintenance-occurrences";

    [DisableConcurrentExecution(timeoutInSeconds: 60 * 60)]
    public Task RunAsync(CancellationToken cancellationToken) => extender.ExtendAllAsync(cancellationToken);
}
