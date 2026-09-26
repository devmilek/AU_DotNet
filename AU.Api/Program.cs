using System.Text.Json.Serialization;
using Hangfire;
using JasperFx.CodeGeneration.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using AU.Api.Authentication;
using AU.Api.Exceptions;
using AU.Api.RateLimiting;
using AU.Api.Storage;
using AU.Application.Channels.Commands.SendTestNotification;
using AU.Application.Monitors.Commands.CreateMonitor;
using AU.Application.Notifications.Events.EmailConfirmationRequested;
using AU.Application.Notifications.Events.MemberInvited;
using AU.Application.Notifications.Events.PasswordResetRequested;
using AU.Application.Notifications.Events.TestNotificationRequested;
using AU.Infrastructure;
using AU.Infrastructure.Persistence;
using Wolverine;
using Wolverine.FluentValidation;
using Wolverine.Http;
using Wolverine.Http.FluentValidation;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

var rabbitConnectionString = builder.Configuration.GetConnectionString("rabbitmq")!;

builder.Services.AddAuth(builder.Configuration);
builder.Services.AddAppOpenTelemetry("AU.Api");
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

builder.Services.AddControllers(options =>
        options.Filters.Add(new ProducesDefaultResponseTypeAttribute(typeof(ProblemDetails))))
    .AddJsonOptions(options => options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.Strict);
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict);
builder.Services.AddOpenApi();
builder.AddPersistence();
builder.AddAppHangfire();
builder.Services.AddRepositories();
builder.Services.AddNotification(builder.Configuration);
builder.Services.AddFileStorage(builder.Configuration);

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

app.UseLocalFileStorage();

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
    app.MapHangfireDashboard("/hangfire", new DashboardOptions { Authorization = [] }).AllowAnonymous();
}

app.Run();