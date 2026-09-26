using AU.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

// linki w mailach buduje zarówno API, jak i notification-worker
const string frontendUrl = "http://localhost:3000";

var postgres = builder
    .AddPostgres("postgres",
        builder.AddParameter("postgres-user"),
        builder.AddParameter("postgres-pass", secret: true))
    // TimescaleDB = Postgres 18 + rozszerzenie timescaledb (hypertabela checków, agregaty ciągłe)
    .WithImage("timescale/timescaledb", "2.30.1-pg18")
    .WithHostPort(5435)
    // osobny wolumen: obraz dopisuje timescaledb do shared_preload_libraries tylko przy inicjalizacji klastra,
    // więc stary wolumen z czystego Postgresa by go nie załadował.
    // WithVolume zamiast WithDataVolume: przy własnym obrazie Aspire montuje /var/lib/postgresql/data,
    // a obrazy Postgres 18+ trzymają dane w /var/lib/postgresql/18/docker i odmawiają startu z mountem pod starą ścieżką
    .WithVolume("uptime-timescaledb-data", "/var/lib/postgresql")
    .WithLifetime(ContainerLifetime.Persistent);

var db = postgres.AddDatabase("app");

var rabbit = builder
    .AddRabbitMQ("rabbitmq",
        builder.AddParameter("rabbitmq-user"),
        builder.AddParameter("rabbitmq-pass", secret: true))
    .WithDataVolume("uptime-rabbitmq-data")
    .WithManagementPlugin(port: 15673)
    .WithLifetime(ContainerLifetime.Persistent);

var mailpit = builder
    .AddContainer("mailpit", "axllent/mailpit")
    .WithHttpEndpoint(port: 8025, targetPort: 8025, name: "ui")
    .WithEndpoint(port: 1025, targetPort: 1025, name: "smtp")
    .WithVolume("uptime-mailpit-data", "/data")
    .WithEnvironment("MP_DATABASE", "/data/mailpit.db")
    .WithEnvironment("MP_SMTP_AUTH_ACCEPT_ANY", "1")
    .WithEnvironment("MP_SMTP_AUTH_ALLOW_INSECURE", "1")
    .WithLifetime(ContainerLifetime.Persistent);

var api = builder.AddProject<Projects.AU_Api>("api")
    .WithBackingServices(db, rabbit)
    .WithMailpit(mailpit)
    .WithEnvironment("Auth__FrontendUrl", frontendUrl);

var apiHttps = api.GetEndpoint("https");
api.WithUrl($"{apiHttps}/scalar", "Scalar")
    .WithUrl($"{apiHttps}/hangfire", "Hangfire");

builder.AddProject<Projects.AU_CheckScheduler>("check-scheduler")
    .WithBackingServices(db, rabbit);

builder.AddProject<Projects.AU_Jobs>("jobs")
    .WithReference(db)
    .WaitFor(db);

builder.AddProject<Projects.AU_CheckWorker>("check-worker")
    .WithBackingServices(db, rabbit);

builder.AddProject<Projects.AU_NotificationWorker>("notification-worker")
    .WithBackingServices(db, rabbit)
    .WithMailpit(mailpit)
    .WithEnvironment("Auth__FrontendUrl", frontendUrl);

#pragma warning disable ASPIREJAVASCRIPT001
var web = builder.AddNextJsApp("web", "../web")
#pragma warning restore ASPIREJAVASCRIPT001
    .WithEndpoint("http", e => e.Port = 3000)
    .WithEnvironment("API_URL", api.GetEndpoint("https"))
    .WaitFor(api)
    .WithEnvironment("NODE_EXTRA_CA_CERTS", Path.GetFullPath("../web/.certs/aspnet-dev.pem"));

builder.Build().Run();