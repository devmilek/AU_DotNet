using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using US.Application.Checks.ReadModels;
using YC.EntityFrameworkCore.TigerData.TimescaleDB;

namespace US.Infrastructure.Persistence.Configurations;

public sealed class MonitorCheckHourlyConfiguration : IEntityTypeConfiguration<MonitorCheckHourly>
{
    public const string ViewName = "monitor_checks_hourly";

    public void Configure(EntityTypeBuilder<MonitorCheckHourly> builder)
    {
        builder.HasNoKey();
        builder.ConfigureRollupColumns();

        // materializedOnly: false = real-time — bieżąca godzina jest doliczana na żywo z surowych checków
        builder.IsContinuousAggregate(
            ViewName,
            """
            SELECT time_bucket(INTERVAL '1 hour', c.checked_at) AS "Bucket",
                   c.monitor_id AS "MonitorId",
                   count(*) AS "TotalChecks",
                   count(*) FILTER (WHERE c.status = 'UP') AS "UpChecks",
                   count(c.response_time_ms) AS "ResponseTimeCount",
                   sum(c.response_time_ms) AS "ResponseTimeSumMs",
                   min(c.response_time_ms) AS "MinResponseTimeMs",
                   max(c.response_time_ms) AS "MaxResponseTimeMs"
            FROM checks AS c
            GROUP BY 1, 2
            """,
            materializedOnly: false);

        // okno odświeżania musi kończyć się przed retencją surowych checków (30 dni),
        // inaczej odświeżenie po usunięciu chunków wyzerowałoby stare kubełki
        builder.HasRefreshPolicy(
            startOffset: TimeSpan.FromDays(3),
            endOffset: TimeSpan.FromHours(1),
            scheduleInterval: TimeSpan.FromMinutes(30));

        builder.HasRetentionPolicy(dropAfter: 2, unit: Every.Year);
    }
}

public sealed class MonitorCheckDailyConfiguration : IEntityTypeConfiguration<MonitorCheckDaily>
{
    public const string ViewName = "monitor_checks_daily";

    public void Configure(EntityTypeBuilder<MonitorCheckDaily> builder)
    {
        builder.HasNoKey();
        builder.ConfigureRollupColumns();

        // hierarchiczny agregat: liczony z godzinowego, nie z surowych checków; bez retencji
        builder.IsContinuousAggregate(
            ViewName,
            $"""
            SELECT time_bucket(INTERVAL '1 day', h."Bucket") AS "Bucket",
                   h."MonitorId",
                   sum(h."TotalChecks")::bigint AS "TotalChecks",
                   sum(h."UpChecks")::bigint AS "UpChecks",
                   sum(h."ResponseTimeCount")::bigint AS "ResponseTimeCount",
                   sum(h."ResponseTimeSumMs")::bigint AS "ResponseTimeSumMs",
                   min(h."MinResponseTimeMs") AS "MinResponseTimeMs",
                   max(h."MaxResponseTimeMs") AS "MaxResponseTimeMs"
            FROM {MonitorCheckHourlyConfiguration.ViewName} AS h
            GROUP BY 1, 2
            """,
            materializedOnly: false);

        builder.HasRefreshPolicy(
            startOffset: TimeSpan.FromDays(3),
            endOffset: TimeSpan.FromHours(1),
            scheduleInterval: TimeSpan.FromHours(1));
    }
}

internal static class MonitorCheckRollupConfigurationExtensions
{
    public static void ConfigureRollupColumns<T>(this EntityTypeBuilder<T> builder)
        where T : MonitorCheckRollup
    {
        // wyliczane w C#, nie ma ich w widoku
        builder.Ignore(x => x.UptimeRatio);
        builder.Ignore(x => x.AverageResponseTimeMs);
    }
}
