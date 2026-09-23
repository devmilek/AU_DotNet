using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace US.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NotificationChannelConfigJsonb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Config",
                table: "notification_channels",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "json");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Config",
                table: "notification_channels",
                type: "json",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "jsonb");
        }
    }
}
