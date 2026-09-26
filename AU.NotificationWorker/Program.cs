using JasperFx.CodeGeneration.Model;
using AU.Application.Incidents.Events.IncidentOpened;
using AU.Application.Notifications.Events.TestNotificationRequested;
using AU.Infrastructure;
using AU.NotificationWorker;
using Wolverine;
using Wolverine.RabbitMQ;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddAppOpenTelemetry("AU.NotificationWorker");

builder.AddPersistence();
builder.Services.AddRepositories();
builder.Services.AddNotification(builder.Configuration);

var rabbitConnectionString = builder.Configuration.GetConnectionString("rabbitmq")!;

builder.UseWolverine(opts =>
{
    opts.UseRuntimeCompilation();
    opts.ServiceLocationPolicy = ServiceLocationPolicy.AlwaysAllowed;
    opts.UseRabbitMq(rabbitConnectionString).AutoProvision();
    
    opts.ListenToRabbitQueue("notifications");
    opts.Discovery.IncludeAssembly(typeof(IncidentOpenedEvent).Assembly);
    opts.Discovery.IncludeAssembly(typeof(TestNotificationEvent).Assembly);
});

var host = builder.Build();
host.Run();