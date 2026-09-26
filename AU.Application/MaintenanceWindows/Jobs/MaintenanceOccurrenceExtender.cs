using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AU.Application.MaintenanceWindows.Jobs;

public sealed class MaintenanceOccurrenceExtender(
    IServiceScopeFactory scopeFactory,
    IMaintenanceWindowRepository windowRepository,
    TimeProvider timeProvider,
    ILogger<MaintenanceOccurrenceExtender> logger)
{
    public async Task ExtendAllAsync(CancellationToken cancellationToken)
    {
        var windowIds = await windowRepository.GetRecurringIdsAsync(cancellationToken);
        var failed = 0;

        foreach (var windowId in windowIds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await ExtendAsync(windowId, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                failed++;
                logger.LogError(ex, "Nie udało się wygenerować wystąpień okna serwisowego {WindowId}", windowId);
            }
        }

        logger.LogInformation(
            "Wygenerowano wystąpienia dla {Succeeded}/{Total} cyklicznych okien serwisowych",
            windowIds.Count - failed, windowIds.Count);
    }

    private async Task ExtendAsync(Guid windowId, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IMaintenanceWindowRepository>();
        var expander = scope.ServiceProvider.GetRequiredService<IRecurrenceExpander>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var now = timeProvider.GetUtcNow();

        var window = await repository.GetForSchedulingAsync(windowId, now, cancellationToken);
        if (window is null) return;

        var removed = MaintenanceSchedule.Sync(window, expander, now);
        repository.RemoveOccurrences(removed);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
