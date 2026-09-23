using JasperFx.CodeGeneration.Model;
using US.Application.Incidents.Events.IncidentOpened;
using US.Application.Notifications.Events.TestNotificationRequested;
using US.Infrastructure;
using US.NotificationWorker;
using Wolverine;
using Wolverine.RabbitMQ;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddAppOpenTelemetry("US.NotificationWorker");

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