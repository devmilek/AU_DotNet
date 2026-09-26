using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using US.Application.Checks.ReadModels;

namespace US.Infrastructure.Persistence.Configurations;

public sealed class MonitorCheckPhasesHourlyConfiguration : IEntityTypeConfiguration<MonitorCheckPhasesHourly>
{
    public const string ViewName = "monitor_check_phases_hourly";

    public static readonly TimeSpan Retention = TimeSpan.FromDays(90);

    public void Configure(EntityTypeBuilder<MonitorCheckPhasesHourly> builder)
    {
        builder.HasNoKey();

        builder.IsContinuousAggregate(
            ViewName,
            """
            SELECT time_bucket(INTERVAL '1 hour', c.checked_at) AS "Bucket",
                   c.monitor_id AS "MonitorId",
                   count(c.dns_ms) FILTER (WHERE c.status = 'UP') AS "DnsCount",
                   sum(c.dns_ms) FILTER (WHERE c.status = 'UP') AS "DnsSumMs",
                   count(c.connect_ms) FILTER (WHERE c.status = 'UP') AS "ConnectCount",
                   sum(c.connect_ms) FILTER (WHERE c.status = 'UP') AS "ConnectSumMs",
                   count(c.tls_ms) FILTER (WHERE c.status = 'UP') AS "TlsCount",
                   sum(c.tls_ms) FILTER (WHERE c.status = 'UP') AS "TlsSumMs",
                   count(c.ttfb_ms) FILTER (WHERE c.status = 'UP') AS "TtfbCount",
                   sum(c.ttfb_ms) FILTER (WHERE c.status = 'UP') AS "TtfbSumMs",
                   count(c.transfer_ms) FILTER (WHERE c.status = 'UP') AS "TransferCount",
                   sum(c.transfer_ms) FILTER (WHERE c.status = 'UP') AS "TransferSumMs"
            FROM checks AS c
            GROUP BY 1, 2
            """,
            materializedOnly: false);

        builder.HasRefreshPolicy(
            startOffset: TimeSpan.FromDays(3),
            endOffset: TimeSpan.FromHours(1),
            scheduleInterval: TimeSpan.FromMinutes(30));

        builder.HasRetentionPolicy(dropAfter: Retention);
    }
}
