using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AU.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CheckPhasesHourlyAggregate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE MATERIALIZED VIEW \"monitor_check_phases_hourly\"\nWITH (timescaledb.continuous, timescaledb.materialized_only = false) AS\nSELECT time_bucket(INTERVAL '1 hour', c.checked_at) AS \"Bucket\",\n       c.monitor_id AS \"MonitorId\",\n       count(c.dns_ms) FILTER (WHERE c.status = 'UP') AS \"DnsCount\",\n       sum(c.dns_ms) FILTER (WHERE c.status = 'UP') AS \"DnsSumMs\",\n       count(c.connect_ms) FILTER (WHERE c.status = 'UP') AS \"ConnectCount\",\n       sum(c.connect_ms) FILTER (WHERE c.status = 'UP') AS \"ConnectSumMs\",\n       count(c.tls_ms) FILTER (WHERE c.status = 'UP') AS \"TlsCount\",\n       sum(c.tls_ms) FILTER (WHERE c.status = 'UP') AS \"TlsSumMs\",\n       count(c.ttfb_ms) FILTER (WHERE c.status = 'UP') AS \"TtfbCount\",\n       sum(c.ttfb_ms) FILTER (WHERE c.status = 'UP') AS \"TtfbSumMs\",\n       count(c.transfer_ms) FILTER (WHERE c.status = 'UP') AS \"TransferCount\",\n       sum(c.transfer_ms) FILTER (WHERE c.status = 'UP') AS \"TransferSumMs\"\nFROM checks AS c\nGROUP BY 1, 2\nWITH NO DATA;");

            migrationBuilder.Sql("SELECT add_continuous_aggregate_policy('\"monitor_check_phases_hourly\"', start_offset => INTERVAL '3 days', end_offset => INTERVAL '01:00:00', schedule_interval => INTERVAL '00:30:00');");

            migrationBuilder.Sql("SELECT add_retention_policy('\"monitor_check_phases_hourly\"', drop_after => INTERVAL '90 days');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP MATERIALIZED VIEW IF EXISTS \"monitor_check_phases_hourly\";");
        }
    }
}
