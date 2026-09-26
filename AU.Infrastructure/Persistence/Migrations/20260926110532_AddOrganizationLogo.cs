using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AU.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationLogo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogoKey",
                table: "Organizations",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoKey",
                table: "Organizations");
        }
    }
}
