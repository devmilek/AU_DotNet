using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using AU.Application;
using AU.Application.Abstractions;
using AU.Application.Channels;
using AU.Application.Checks;
using AU.Application.Checks.Statistics;
using AU.Application.MaintenanceWindows;
using AU.Application.Monitors;
using AU.Application.Notifications;
using AU.Application.Organizations;
using AU.Infrastructure.Checkers;
using AU.Infrastructure.Identity;
using AU.Infrastructure.Notifications;
using AU.Infrastructure.Persistence;
using AU.Infrastructure.Persistence.Readers;
using AU.Infrastructure.Persistence.Repositories;
using AU.Infrastructure.Scheduling;
using AU.Infrastructure.Security;

namespace AU.Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddPersistence(this IHostApplicationBuilder builder)
    {
        // UseTimescaleDb musi być po UseNpgsql — Aspire woła ten delegat po skonfigurowaniu Npgsql
        builder.AddNpgsqlDbContext<AppDbContext>("app",
            configureDbContextOptions: options => options.UseTimescaleDb());

        return builder;
    }
    
    /// <summary>
    /// Wspólny key ring Data Protection (klucze w bazie) — API szyfruje nim sekrety monitorów,
    /// a CheckWorker je odszyfrowuje, więc nazwa aplikacji i magazyn kluczy muszą być identyczne.
    /// </summary>
    public static IServiceCollection AddAppDataProtection(this IServiceCollection services)
    {
        services.AddDataProtection()
            .SetApplicationName("AsterioUptime")
            .PersistKeysToDbContext<AppDbContext>();

        services.AddSingleton<ISecretProtector, DataProtectionSecretProtector>();

        return services;
    }

    public static IdentityBuilder AddIdentityPersistence(this IServiceCollection services)
    {
        services.AddScoped<IUserLookup, UserLookup>();

        return services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.Password.RequiredLength = 10;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;

                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

                options.SignIn.RequireConfirmedEmail = true;
                
                options.Tokens.PasswordResetTokenProvider = "PasswordResetProvider";
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
    }    
    
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IMonitorRepository, MonitorRepository>();
        services.AddScoped<IChecksRepository, ChecksRepository>();
        services.AddScoped<IIncidentRepository, IncidentRepository>();
        services.AddScoped<INotificationChannelRepository, NotificationChannelRepository>();
        services.AddScoped<IMonitorNotificationChannelRepository, MonitorNotificationChannelRepository>();
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<IOrganizationMemberRepository, OrganizationMemberRepository>();
        services.AddScoped<IInvitationRepository, InvitationRepository>();
        services.AddScoped<IInvitationTokenService, InvitationTokenService>();
        services.AddScoped<IMaintenanceWindowRepository, MaintenanceWindowRepository>();
        services.AddSingleton<IRecurrenceExpander, IcalRecurrenceExpander>();
        
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IMonitorStatisticsReader, MonitorStatisticsReader>();
        services.AddScoped<IPhaseTimingsReader, PhaseTimingsReader>();
        services.TryAddSingleton(TimeProvider.System);

        return services;
    }
    
    public static IHostApplicationBuilder AddAppHangfire(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("app")!;

        builder.Services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(
                options => options.UseNpgsqlConnection(connectionString),
                new PostgreSqlStorageOptions { SchemaName = "hangfire" }));

        return builder;
    }

    public static IServiceCollection AddCheckers(this IServiceCollection services)
    {
        services.AddScoped<IMonitorChecker, HttpMonitorChecker>();
        services.AddScoped<IMonitorChecker, TcpMonitorChecker>();

        return services;
    }
    
    public static IServiceCollection AddNotification(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IEmailTemplateRenderer, ScribanEmailTemplateRenderer>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.Configure<SmtpOptions>(configuration.GetSection("Smtp"));
        services.Configure<FrontendOptions>(configuration.GetSection(FrontendOptions.SectionName));
        services.AddScoped<INotificationSender, NotificationSender>();

        return services;
    }
    
    public static IServiceCollection AddAppOpenTelemetry(this IServiceCollection services, string serviceName)
    {
        services
            .AddOpenTelemetry()
            .ConfigureResource(resource =>
                resource.AddService(serviceName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSource("Wolverine")
                    .AddSource("AU.Api")
                    .AddOtlpExporter();
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddMeter("Wolverine*")
                    .AddOtlpExporter();
            });

        return services;
    }
    
    public static ILoggingBuilder AddAppOpenTelemetry(
        this ILoggingBuilder logging)
    {
        logging.AddOpenTelemetry(options =>
        {
            options.IncludeFormattedMessage = true;
            options.IncludeScopes = true;
            options.AddOtlpExporter();
        });

        return logging;
    }
}