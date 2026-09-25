using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace US.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MaintenanceWindows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "StartedInMaintenance",
                table: "incidents",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "maintenance_occurrence_id",
                table: "checks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "was_in_maintenance",
                table: "checks",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "maintenance_windows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TimeZoneId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StartsAtLocal = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    RecurrenceRule = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RecurrenceEndLocal = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    SuppressNotifications = table.Column<bool>(type: "boolean", nullable: false),
                    ExcludeFromSla = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintenance_windows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintenance_windows_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintenance_occurrences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MaintenanceWindowId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ContentLockedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    StartsAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndsAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ScheduledStartUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintenance_occurrences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintenance_occurrences_maintenance_windows_MaintenanceWind~",
                        column: x => x.MaintenanceWindowId,
                        principalTable: "maintenance_windows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "maintenance_window_monitors",
                columns: table => new
                {
                    MaintenanceWindowId = table.Column<Guid>(type: "uuid", nullable: false),
                    MonitorId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintenance_window_monitors", x => new { x.MaintenanceWindowId, x.MonitorId });
                    table.ForeignKey(
                        name: "FK_maintenance_window_monitors_maintenance_windows_Maintenance~",
                        column: x => x.MaintenanceWindowId,
                        principalTable: "maintenance_windows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_maintenance_window_monitors_monitors_MonitorId",
                        column: x => x.MonitorId,
                        principalTable: "monitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_checks_maintenance_occurrence_id",
                table: "checks",
                column: "maintenance_occurrence_id",
                filter: "maintenance_occurrence_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_maintenance_occurrences_MaintenanceWindowId_ScheduledStartU~",
                table: "maintenance_occurrences",
                columns: new[] { "MaintenanceWindowId", "ScheduledStartUtc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintenance_occurrences_StartsAtUtc_EndsAtUtc",
                table: "maintenance_occurrences",
                columns: new[] { "StartsAtUtc", "EndsAtUtc" },
                filter: "\"Status\" = 'Scheduled'");

            migrationBuilder.CreateIndex(
                name: "IX_maintenance_window_monitors_MonitorId",
                table: "maintenance_window_monitors",
                column: "MonitorId");

            migrationBuilder.CreateIndex(
                name: "IX_maintenance_windows_OrganizationId",
                table: "maintenance_windows",
                column: "OrganizationId",
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_checks_maintenance_occurrences_maintenance_occurrence_id",
                table: "checks",
                column: "maintenance_occurrence_id",
                principalTable: "maintenance_occurrences",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_checks_maintenance_occurrences_maintenance_occurrence_id",
                table: "checks");

            migrationBuilder.DropTable(
                name: "maintenance_occurrences");

            migrationBuilder.DropTable(
                name: "maintenance_window_monitors");

            migrationBuilder.DropTable(
                name: "maintenance_windows");

            migrationBuilder.DropIndex(
                name: "IX_checks_maintenance_occurrence_id",
                table: "checks");

            migrationBuilder.DropColumn(
                name: "StartedInMaintenance",
                table: "incidents");

            migrationBuilder.DropColumn(
                name: "maintenance_occurrence_id",
                table: "checks");

            migrationBuilder.DropColumn(
                name: "was_in_maintenance",
                table: "checks");
        }
    }
}
