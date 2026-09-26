using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AU.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ResponseTimeFromSuccessfulChecks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP MATERIALIZED VIEW IF EXISTS \"monitor_checks_daily\";");

            migrationBuilder.Sql("DROP MATERIALIZED VIEW IF EXISTS \"monitor_checks_hourly\";");

            migrationBuilder.Sql("CREATE MATERIALIZED VIEW \"monitor_checks_hourly\"\nWITH (timescaledb.continuous, timescaledb.materialized_only = false) AS\nSELECT time_bucket(INTERVAL '1 hour', c.checked_at) AS \"Bucket\",\n       c.monitor_id AS \"MonitorId\",\n       count(*) AS \"TotalChecks\",\n       count(*) FILTER (WHERE c.status = 'UP') AS \"UpChecks\",\n       count(c.response_time_ms) FILTER (WHERE c.status = 'UP') AS \"ResponseTimeCount\",\n       sum(c.response_time_ms) FILTER (WHERE c.status = 'UP') AS \"ResponseTimeSumMs\",\n       min(c.response_time_ms) FILTER (WHERE c.status = 'UP') AS \"MinResponseTimeMs\",\n       max(c.response_time_ms) FILTER (WHERE c.status = 'UP') AS \"MaxResponseTimeMs\"\nFROM checks AS c\nGROUP BY 1, 2\nWITH NO DATA;");

            migrationBuilder.Sql("CREATE MATERIALIZED VIEW \"monitor_checks_daily\"\nWITH (timescaledb.continuous, timescaledb.materialized_only = false) AS\nSELECT time_bucket(INTERVAL '1 day', h.\"Bucket\") AS \"Bucket\",\n       h.\"MonitorId\",\n       sum(h.\"TotalChecks\")::bigint AS \"TotalChecks\",\n       sum(h.\"UpChecks\")::bigint AS \"UpChecks\",\n       sum(h.\"ResponseTimeCount\")::bigint AS \"ResponseTimeCount\",\n       sum(h.\"ResponseTimeSumMs\")::bigint AS \"ResponseTimeSumMs\",\n       min(h.\"MinResponseTimeMs\") AS \"MinResponseTimeMs\",\n       max(h.\"MaxResponseTimeMs\") AS \"MaxResponseTimeMs\"\nFROM monitor_checks_hourly AS h\nGROUP BY 1, 2\nWITH NO DATA;");

            migrationBuilder.Sql("SELECT add_continuous_aggregate_policy('\"monitor_checks_daily\"', start_offset => INTERVAL '3 days', end_offset => INTERVAL '01:00:00', schedule_interval => INTERVAL '01:00:00');");

            migrationBuilder.Sql("SELECT add_continuous_aggregate_policy('\"monitor_checks_hourly\"', start_offset => INTERVAL '3 days', end_offset => INTERVAL '01:00:00', schedule_interval => INTERVAL '00:30:00');");

            migrationBuilder.Sql("SELECT add_retention_policy('\"monitor_checks_hourly\"', drop_after => INTERVAL '2 years');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP MATERIALIZED VIEW IF EXISTS \"monitor_checks_daily\";");

            migrationBuilder.Sql("DROP MATERIALIZED VIEW IF EXISTS \"monitor_checks_hourly\";");

            migrationBuilder.Sql("CREATE MATERIALIZED VIEW \"monitor_checks_hourly\"\nWITH (timescaledb.continuous, timescaledb.materialized_only = false) AS\nSELECT time_bucket(INTERVAL '1 hour', c.checked_at) AS \"Bucket\",\n       c.monitor_id AS \"MonitorId\",\n       count(*) AS \"TotalChecks\",\n       count(*) FILTER (WHERE c.status = 'UP') AS \"UpChecks\",\n       count(c.response_time_ms) AS \"ResponseTimeCount\",\n       sum(c.response_time_ms) AS \"ResponseTimeSumMs\",\n       min(c.response_time_ms) AS \"MinResponseTimeMs\",\n       max(c.response_time_ms) AS \"MaxResponseTimeMs\"\nFROM checks AS c\nGROUP BY 1, 2\nWITH NO DATA;");

            migrationBuilder.Sql("CREATE MATERIALIZED VIEW \"monitor_checks_daily\"\nWITH (timescaledb.continuous, timescaledb.materialized_only = false) AS\nSELECT time_bucket(INTERVAL '1 day', h.\"Bucket\") AS \"Bucket\",\n       h.\"MonitorId\",\n       sum(h.\"TotalChecks\")::bigint AS \"TotalChecks\",\n       sum(h.\"UpChecks\")::bigint AS \"UpChecks\",\n       sum(h.\"ResponseTimeCount\")::bigint AS \"ResponseTimeCount\",\n       sum(h.\"ResponseTimeSumMs\")::bigint AS \"ResponseTimeSumMs\",\n       min(h.\"MinResponseTimeMs\") AS \"MinResponseTimeMs\",\n       max(h.\"MaxResponseTimeMs\") AS \"MaxResponseTimeMs\"\nFROM monitor_checks_hourly AS h\nGROUP BY 1, 2\nWITH NO DATA;");

            migrationBuilder.Sql("SELECT add_continuous_aggregate_policy('\"monitor_checks_daily\"', start_offset => INTERVAL '3 days', end_offset => INTERVAL '01:00:00', schedule_interval => INTERVAL '01:00:00');");

            migrationBuilder.Sql("SELECT add_continuous_aggregate_policy('\"monitor_checks_hourly\"', start_offset => INTERVAL '3 days', end_offset => INTERVAL '01:00:00', schedule_interval => INTERVAL '00:30:00');");

            migrationBuilder.Sql("SELECT add_retention_policy('\"monitor_checks_hourly\"', drop_after => INTERVAL '2 years');");
        }
    }
}
