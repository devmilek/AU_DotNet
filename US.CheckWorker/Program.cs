using JasperFx.CodeGeneration.Model;
using US.Application.Checks.Commands.CheckMonitor;
using US.Application.Incidents.Events.IncidentOpened;
using US.Application.Incidents.Events.IncidentResolved;
using US.Application.Incidents.Events.MonitorCheckCompleted;
using US.Infrastructure;
using Wolverine;
using Wolverine.RabbitMQ;

var builder = Host.CreateApplicationBuilder(args);
builder.AddPersistence();
builder.Services.AddRepositories();
builder.Services.AddCheckers();
builder.Services.AddAppOpenTelemetry("US.CheckWorker");

var rabbitConnectionString = builder.Configuration.GetConnectionString("rabbitmq")!;

builder.UseWolverine(opts =>
{
    opts.UseRuntimeCompilation();
    opts.ServiceLocationPolicy = ServiceLocationPolicy.AlwaysAllowed;
    opts.UseRabbitMq(rabbitConnectionString).AutoProvision();

    opts.Discovery.IncludeAssembly(typeof(CheckMonitorHandler).Assembly);
    
    opts.ListenToRabbitQueue("monitor.check");
    opts.ListenToRabbitQueue("incident.evaluate");
    
    opts.PublishMessage<MonitorCheckCompletedEvent>().ToRabbitQueue("incident.evaluate");
    opts.PublishMessage<IncidentOpenedEvent>().ToRabbitQueue("notifications");
    opts.PublishMessage<IncidentResolvedEvent>().ToRabbitQueue("notifications");
});

builder.Logging.AddAppOpenTelemetry();

var host = builder.Build();
host.Run();