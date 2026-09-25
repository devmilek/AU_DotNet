using Hangfire;
using US.Application.MaintenanceWindows.Jobs;
using US.Infrastructure;
using US.Jobs;

var builder = Host.CreateApplicationBuilder(args);

builder.AddPersistence();
builder.AddAppHangfire();
builder.Services.AddRepositories();
builder.Services.AddAppOpenTelemetry("US.Jobs");
builder.Logging.AddAppOpenTelemetry();

builder.Services.AddScoped<MaintenanceOccurrenceExtender>();
builder.Services.AddScoped<MaintenanceOccurrencesJob>();

builder.Services.AddHangfireServer();

var host = builder.Build();

host.Services.GetRequiredService<IRecurringJobManager>().AddOrUpdate<MaintenanceOccurrencesJob>(
    MaintenanceOccurrencesJob.Id,
    job => job.RunAsync(CancellationToken.None),
    Cron.Daily(3));

host.Run();
