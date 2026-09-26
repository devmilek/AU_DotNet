using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AU.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class IncidentDetailsAndAcknowledgement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AcknowledgedByUserId",
                table: "incidents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cause",
                table: "incidents",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "incidents",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_incidents_AcknowledgedByUserId",
                table: "incidents",
                column: "AcknowledgedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_incidents_AspNetUsers_AcknowledgedByUserId",
                table: "incidents",
                column: "AcknowledgedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.Sql("""
                UPDATE incidents i
                SET "Cause" = (
                    SELECT LEFT(c.error_message, 1000)
                    FROM checks c
                    WHERE c.monitor_id = i."MonitorId"
                      AND c.checked_at <= i."StartedAt"
                      AND c.error_message IS NOT NULL
                    ORDER BY c.checked_at DESC
                    LIMIT 1
                )
                WHERE i."Cause" IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_incidents_AspNetUsers_AcknowledgedByUserId",
                table: "incidents");

            migrationBuilder.DropIndex(
                name: "IX_incidents_AcknowledgedByUserId",
                table: "incidents");

            migrationBuilder.DropColumn(
                name: "AcknowledgedByUserId",
                table: "incidents");

            migrationBuilder.DropColumn(
                name: "Cause",
                table: "incidents");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "incidents");
        }
    }
}
