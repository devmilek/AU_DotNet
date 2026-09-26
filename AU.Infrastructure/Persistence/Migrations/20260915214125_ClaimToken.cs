using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AU.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ClaimToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClaimToken",
                table: "monitors",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClaimToken",
                table: "monitors");
        }
    }
}
