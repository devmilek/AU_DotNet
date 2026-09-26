using AU.Application.Checks.Commands.CheckMonitor;
using AU.CheckScheduler;
using AU.Infrastructure;
using Wolverine;
using Wolverine.RabbitMQ;

var builder = Host.CreateApplicationBuilder(args);
builder.AddPersistence();
builder.Services.AddAppOpenTelemetry("AU.CheckScheduler");
builder.Services.AddScoped<MonitorClaimer>();
builder.Services.AddHostedService<Worker>();

var rabbitConnectionString = builder.Configuration.GetConnectionString("rabbitmq")!;


builder.UseWolverine(opts =>
{
    opts.UseRabbitMq(rabbitConnectionString).AutoProvision();
    
    opts.PublishMessage<CheckMonitorCommand>().ToRabbitQueue("monitor.check");
});

builder.Logging.AddAppOpenTelemetry();

var host = builder.Build();
host.Run();