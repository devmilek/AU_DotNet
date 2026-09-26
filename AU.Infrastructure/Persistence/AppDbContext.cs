using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using AU.Application.Checks.ReadModels;
using AU.Domain.Entities;
using AU.Infrastructure.Identity;
using Monitor = AU.Domain.Entities.Monitor;

namespace AU.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityUserContext<ApplicationUser, Guid>(options), IDataProtectionKeyContext
{
    public DbSet<Monitor> Monitors => Set<Monitor>();
    public DbSet<MonitorState> MonitorStates => Set<MonitorState>();
    public DbSet<Check> MonitorChecks => Set<Check>();
    public DbSet<MonitorCheckHourly> MonitorChecksHourly => Set<MonitorCheckHourly>();
    public DbSet<MonitorCheckDaily> MonitorChecksDaily => Set<MonitorCheckDaily>();
    public DbSet<MonitorCheckPhasesHourly> MonitorCheckPhasesHourly => Set<MonitorCheckPhasesHourly>();
    public DbSet<Incident> Incidents => Set<Incident>();
    public DbSet<IncidentNotification> IncidentNotifications => Set<IncidentNotification>();
    public DbSet<NotificationChannel> NotificationChannels => Set<NotificationChannel>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<OrganizationMember> OrganizationMembers => Set<OrganizationMember>();
    public DbSet<Invitation> Invitations => Set<Invitation>();
    public DbSet<MonitorNotificationChannel> MonitorNotificationChannels => Set<MonitorNotificationChannel>();
    public DbSet<MaintenanceWindow> MaintenanceWindows => Set<MaintenanceWindow>();
    public DbSet<MaintenanceWindowMonitor> MaintenanceWindowMonitors => Set<MaintenanceWindowMonitor>();
    public DbSet<MaintenanceOccurrence> MaintenanceOccurrences => Set<MaintenanceOccurrence>();
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly);
    }
}

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder
            .UseNpgsql("Host=localhost;Port=5435;Database=app;Username=postgres;Password=milek123")
            .UseTimescaleDb();

        return new AppDbContext(optionsBuilder.Options);
    }
}