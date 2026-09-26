using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AU.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TimescaleChecks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS timescaledb;");

            migrationBuilder.DropForeignKey(
                name: "FK_checks_monitors_MonitorId",
                table: "checks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_checks",
                table: "checks");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "checks",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "checks",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "StatusCode",
                table: "checks",
                newName: "status_code");

            migrationBuilder.RenameColumn(
                name: "ResponseTimeMs",
                table: "checks",
                newName: "response_time_ms");

            migrationBuilder.RenameColumn(
                name: "MonitorId",
                table: "checks",
                newName: "monitor_id");

            migrationBuilder.RenameColumn(
                name: "ErrorMessage",
                table: "checks",
                newName: "error_message");

            migrationBuilder.RenameColumn(
                name: "CheckedAt",
                table: "checks",
                newName: "checked_at");

            migrationBuilder.RenameIndex(
                name: "IX_checks_MonitorId_CheckedAt",
                table: "checks",
                newName: "IX_checks_monitor_id_checked_at");

            migrationBuilder.AlterTable(
                name: "checks")
                .Annotation("TimescaleDb:Columnstore:Enabled", true)
                .Annotation("TimescaleDb:Columnstore:OrderBy", "checked_at DESC")
                .Annotation("TimescaleDb:Columnstore:SegmentBy", "monitor_id")
                .Annotation("TimescaleDb:ColumnstorePolicy:After", "1 day")
                .Annotation("TimescaleDb:Hypertable:ChunkInterval", "1 day")
                .Annotation("TimescaleDb:Hypertable:PartitionColumn", "checked_at")
                .Annotation("TimescaleDb:IsHypertable", true)
                .Annotation("TimescaleDb:RetentionPolicy:DropAfter", "30 days");

            migrationBuilder.AddPrimaryKey(
                name: "PK_checks",
                table: "checks",
                columns: new[] { "id", "checked_at" });

            migrationBuilder.AddForeignKey(
                name: "FK_checks_monitors_monitor_id",
                table: "checks",
                column: "monitor_id",
                principalTable: "monitors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.Sql("CREATE MATERIALIZED VIEW \"monitor_checks_hourly\"\nWITH (timescaledb.continuous, timescaledb.materialized_only = false) AS\nSELECT time_bucket(INTERVAL '1 hour', c.checked_at) AS \"Bucket\",\n       c.monitor_id AS \"MonitorId\",\n       count(*) AS \"TotalChecks\",\n       count(*) FILTER (WHERE c.status = 'UP') AS \"UpChecks\",\n       count(c.response_time_ms) AS \"ResponseTimeCount\",\n       sum(c.response_time_ms) AS \"ResponseTimeSumMs\",\n       min(c.response_time_ms) AS \"MinResponseTimeMs\",\n       max(c.response_time_ms) AS \"MaxResponseTimeMs\"\nFROM checks AS c\nGROUP BY 1, 2\nWITH NO DATA;");

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

            migrationBuilder.DropForeignKey(
                name: "FK_checks_monitors_monitor_id",
                table: "checks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_checks",
                table: "checks");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "checks",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "checks",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "status_code",
                table: "checks",
                newName: "StatusCode");

            migrationBuilder.RenameColumn(
                name: "response_time_ms",
                table: "checks",
                newName: "ResponseTimeMs");

            migrationBuilder.RenameColumn(
                name: "monitor_id",
                table: "checks",
                newName: "MonitorId");

            migrationBuilder.RenameColumn(
                name: "error_message",
                table: "checks",
                newName: "ErrorMessage");

            migrationBuilder.RenameColumn(
                name: "checked_at",
                table: "checks",
                newName: "CheckedAt");

            migrationBuilder.RenameIndex(
                name: "IX_checks_monitor_id_checked_at",
                table: "checks",
                newName: "IX_checks_MonitorId_CheckedAt");

            migrationBuilder.AlterTable(
                name: "checks")
                .OldAnnotation("TimescaleDb:Columnstore:Enabled", true)
                .OldAnnotation("TimescaleDb:Columnstore:OrderBy", "checked_at DESC")
                .OldAnnotation("TimescaleDb:Columnstore:SegmentBy", "monitor_id")
                .OldAnnotation("TimescaleDb:ColumnstorePolicy:After", "1 day")
                .OldAnnotation("TimescaleDb:Hypertable:ChunkInterval", "1 day")
                .OldAnnotation("TimescaleDb:Hypertable:PartitionColumn", "checked_at")
                .OldAnnotation("TimescaleDb:IsHypertable", true)
                .OldAnnotation("TimescaleDb:RetentionPolicy:DropAfter", "30 days");

            migrationBuilder.AddPrimaryKey(
                name: "PK_checks",
                table: "checks",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_checks_monitors_MonitorId",
                table: "checks",
                column: "MonitorId",
                principalTable: "monitors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
