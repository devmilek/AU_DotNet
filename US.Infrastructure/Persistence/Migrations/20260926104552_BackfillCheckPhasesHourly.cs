using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace US.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BackfillCheckPhasesHourly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "CALL refresh_continuous_aggregate('\"monitor_check_phases_hourly\"', NULL, now() - INTERVAL '1 hour');",
                suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
