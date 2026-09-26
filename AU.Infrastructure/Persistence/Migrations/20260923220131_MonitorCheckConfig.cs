using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AU.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MonitorCheckConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Config",
                table: "monitors",
                type: "jsonb",
                nullable: true);

            // istniejące monitory HTTP dostają domyślną konfigurację (GET, follow redirects, 200-299)
            migrationBuilder.Sql("""
                UPDATE monitors
                SET "Config" = '{"$type":"http","method":"Get","followRedirects":true,"acceptedStatusCodes":[{"from":200,"to":299}],"auth":null}'::jsonb
                WHERE "Type" = 'Http' AND "Config" IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Config",
                table: "monitors");
        }
    }
}
