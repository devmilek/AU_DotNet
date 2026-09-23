using System.Text.Json.Serialization;
using JasperFx.CodeGeneration.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using US.Api.Authentication;
using US.Api.Exceptions;
using US.Api.RateLimiting;
using US.Application.Channels.Commands.SendTestNotification;
using US.Application.Monitors.Commands.CreateMonitor;
using US.Application.Notifications.Events.EmailConfirmationRequested;
using US.Application.Notifications.Events.MemberInvited;
using US.Application.Notifications.Events.PasswordResetRequested;
using US.Application.Notifications.Events.TestNotificationRequested;
using US.Infrastructure;
using US.Infrastructure.Persistence;
using Wolverine;
using Wolverine.FluentValidation;
using Wolverine.Http;
using Wolverine.Http.FluentValidation;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

var rabbitConnectionString = builder.Configuration.GetConnectionString("rabbitmq")!;

builder.Services.AddAuth(builder.Configuration);
builder.Services.AddAppOpenTelemetry("US.Api");
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<ForbiddenExceptionHandler>();
builder.Services.AddExceptionHandler<ConflictExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddAppRateLimiting();

builder.Host.UseWolverine(opts =>
{
    opts.UseRuntimeCompilation();
    opts.ServiceLocationPolicy = ServiceLocationPolicy.AlwaysAllowed;
    opts.UseRabbitMq(rabbitConnectionString).AutoProvision();

    opts.Discovery.IncludeAssembly(typeof(CreateMonitorHandler).Assembly);
    opts.Discovery.IncludeAssembly(typeof(TestNotificationHandler).Assembly);
    opts.PublishMessage<TestNotificationEvent>().ToRabbitQueue("notifications");
    opts.PublishMessage<EmailConfirmationRequestedEvent>().ToRabbitQueue("notifications");
    opts.PublishMessage<MemberInvitedEvent>().ToRabbitQueue("notifications");
    opts.PublishMessage<PasswordResetRequestedEvent>().ToRabbitQueue("notifications");
    
    opts.UseFluentValidation();
});

builder.Logging.AddAppOpenTelemetry();

// każdy błąd (4xx/5xx) jest zwracany jako ProblemDetails — opisujemy to w OpenAPI jako odpowiedź "default"
builder.Services.AddControllers(options =>
    options.Filters.Add(new ProducesDefaultResponseTypeAttribute(typeof(ProblemDetails))));
builder.Services.AddOpenApi();
builder.AddPersistence();
builder.Services.AddRepositories();
builder.Services.AddNotification(builder.Configuration);

var app = builder.Build();

app.UseForwardedHeaders();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseExceptionHandler();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseRateLimiter(); 
app.UseCors("frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference().AllowAnonymous();
}

app.Run();