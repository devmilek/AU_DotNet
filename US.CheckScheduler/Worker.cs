using Microsoft.EntityFrameworkCore;
using US.Application.Checks.Commands.CheckMonitor;
using US.Infrastructure.Persistence;
using Wolverine;

namespace US.CheckScheduler;

public class Worker(IServiceProvider serviceProvider, ILogger<Worker> logger) : BackgroundService
{
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

                var claimedIds = await claimer.ClaimDueMonitorsAsync(500);
                var now = DateTimeOffset.UtcNow;

                if (claimedIds.Count == 0)
                {
                    logger.LogInformation("No monitors due for check at {time}", now);
                }

                ;

                await Task.WhenAll(claimedIds.Select(id =>
                    bus.PublishAsync(new CheckMonitorCommand(id)).AsTask()));

                logger.LogInformation("Scheduled {count} monitors for check at {time}", claimedIds.Count, now);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to publish check commands, will retry next tick");

            }
        }
    }
}