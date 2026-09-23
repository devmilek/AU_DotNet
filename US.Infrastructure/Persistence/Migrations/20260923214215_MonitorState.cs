using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace US.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MonitorState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "monitor_states",
                columns: table => new
                {
                    MonitorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsecutiveSuccesses = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ConsecutiveFailures = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    NextCheckAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_monitor_states", x => x.MonitorId);
                    table.ForeignKey(
                        name: "FK_monitor_states_monitors_MonitorId",
                        column: x => x.MonitorId,
                        principalTable: "monitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_monitor_states_NextCheckAt",
                table: "monitor_states",
                column: "NextCheckAt");

            // przeniesienie stanu istniejących monitorów, zanim kolumny znikną z monitors
            migrationBuilder.Sql("""
                INSERT INTO monitor_states ("MonitorId", "ConsecutiveSuccesses", "ConsecutiveFailures", "NextCheckAt", "UpdatedAt")
                SELECT "Id", "ConsecutiveSuccesses", "ConsecutiveFailures", "NextCheckAt", now()
                FROM monitors;
                """);

            migrationBuilder.DropColumn(
                name: "ClaimToken",
                table: "monitors");

            migrationBuilder.DropColumn(
                name: "ConsecutiveFailures",
                table: "monitors");

            migrationBuilder.DropColumn(
                name: "ConsecutiveSuccesses",
                table: "monitors");

            migrationBuilder.DropColumn(
                name: "NextCheckAt",
                table: "monitors");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClaimToken",
                table: "monitors",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConsecutiveFailures",
                table: "monitors",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ConsecutiveSuccesses",
                table: "monitors",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NextCheckAt",
                table: "monitors",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.Sql("""
                UPDATE monitors AS m
                SET "ConsecutiveSuccesses" = s."ConsecutiveSuccesses",
                    "ConsecutiveFailures" = s."ConsecutiveFailures",
                    "NextCheckAt" = s."NextCheckAt"
                FROM monitor_states AS s
                WHERE s."MonitorId" = m."Id";
                """);

            migrationBuilder.DropTable(
                name: "monitor_states");
        }
    }
}
