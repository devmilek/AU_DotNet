using US.Application.Checks.Commands.CheckMonitor;
using US.CheckScheduler;
using US.Infrastructure;
using Wolverine;
using Wolverine.RabbitMQ;

var builder = Host.CreateApplicationBuilder(args);
builder.AddPersistence();
builder.Services.AddAppOpenTelemetry("US.CheckScheduler");
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