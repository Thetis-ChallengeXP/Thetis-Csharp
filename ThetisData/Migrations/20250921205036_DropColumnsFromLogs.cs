using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThetisData.Migrations
{
    /// <inheritdoc />
    public partial class DropColumnsFromLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IP_ORIGEM",
                table: "THETIS_LOGS_RECOMENDACAO");

            migrationBuilder.DropColumn(
                name: "USER_AGENT",
                table: "THETIS_LOGS_RECOMENDACAO");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IP_ORIGEM",
                table: "THETIS_LOGS_RECOMENDACAO",
                type: "NVARCHAR2(45)",
                maxLength: 45,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "USER_AGENT",
                table: "THETIS_LOGS_RECOMENDACAO",
                type: "NVARCHAR2(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }
    }
}
