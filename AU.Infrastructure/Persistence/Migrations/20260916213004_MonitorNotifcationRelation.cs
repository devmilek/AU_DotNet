using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AU.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MonitorNotifcationRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "monitor_notification_channels",
                columns: table => new
                {
                    MonitorId = table.Column<Guid>(type: "uuid", nullable: false),
                    NotificationChannelId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_monitor_notification_channels", x => new { x.MonitorId, x.NotificationChannelId });
                    table.ForeignKey(
                        name: "FK_monitor_notification_channels_monitors_MonitorId",
                        column: x => x.MonitorId,
                        principalTable: "monitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_monitor_notification_channels_notification_channels_Notific~",
                        column: x => x.NotificationChannelId,
                        principalTable: "notification_channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_monitor_notification_channels_NotificationChannelId",
                table: "monitor_notification_channels",
                column: "NotificationChannelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "monitor_notification_channels");
        }
    }
}
