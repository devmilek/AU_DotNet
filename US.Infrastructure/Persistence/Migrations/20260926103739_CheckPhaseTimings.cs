using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace US.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CheckPhaseTimings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "connect_ms",
                table: "checks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "dns_ms",
                table: "checks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "tls_ms",
                table: "checks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "transfer_ms",
                table: "checks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ttfb_ms",
                table: "checks",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "connect_ms",
                table: "checks");

            migrationBuilder.DropColumn(
                name: "dns_ms",
                table: "checks");

            migrationBuilder.DropColumn(
                name: "tls_ms",
                table: "checks");

            migrationBuilder.DropColumn(
                name: "transfer_ms",
                table: "checks");

            migrationBuilder.DropColumn(
                name: "ttfb_ms",
                table: "checks");
        }
    }
}
