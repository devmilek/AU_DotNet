using Microsoft.EntityFrameworkCore;
using US.Application.Checks.Commands.CheckMonitor;
using US.Infrastructure.Persistence;
using Wolverine;

namespace US.CheckScheduler;

public class Worker(IServiceProvider serviceProvider, ILogger<Worker> logger) : BackgroundService
{
    private const int BatchSize = 500;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await using var scope = serviceProvider.CreateAsyncScope();
                var claimer = scope.ServiceProvider.GetRequiredService<MonitorClaimer>();
                var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

                // pełny batch = pewnie jest zaległość, więc dociągamy od razu zamiast czekać na następny tick
                List<Guid> claimedIds;
                do
                {
                    claimedIds = await claimer.ClaimDueMonitorsAsync(BatchSize, stoppingToken);
                    if (claimedIds.Count == 0)
                    {
                        logger.LogDebug("No monitors due for check");
                        break;
                    }

                    await Task.WhenAll(claimedIds.Select(id =>
                        bus.PublishAsync(new CheckMonitorCommand(id)).AsTask()));

                    logger.LogInformation("Scheduled {count} monitors for check", claimedIds.Count);
                } while (claimedIds.Count == BatchSize && !stoppingToken.IsCancellationRequested);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to publish check commands, will retry next tick");

            }
        }
    }
}