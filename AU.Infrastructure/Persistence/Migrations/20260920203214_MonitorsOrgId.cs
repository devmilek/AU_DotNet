using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AU.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MonitorsOrgId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "notification_channels",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "monitors",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_notification_channels_OrganizationId",
                table: "notification_channels",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_monitors_OrganizationId",
                table: "monitors",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_monitors_Organizations_OrganizationId",
                table: "monitors",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_notification_channels_Organizations_OrganizationId",
                table: "notification_channels",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_monitors_Organizations_OrganizationId",
                table: "monitors");

            migrationBuilder.DropForeignKey(
                name: "FK_notification_channels_Organizations_OrganizationId",
                table: "notification_channels");

            migrationBuilder.DropIndex(
                name: "IX_notification_channels_OrganizationId",
                table: "notification_channels");

            migrationBuilder.DropIndex(
                name: "IX_monitors_OrganizationId",
                table: "monitors");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "notification_channels");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "monitors");
        }
    }
}
