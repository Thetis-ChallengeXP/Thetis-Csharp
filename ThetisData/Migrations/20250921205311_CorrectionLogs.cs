using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThetisData.Migrations
{
    /// <inheritdoc />
    public partial class CorrectionLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ERRO_MENSAGEM",
                table: "THETIS_LOGS_RECOMENDACAO",
                type: "NVARCHAR2(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(1000)",
                oldMaxLength: 1000);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ERRO_MENSAGEM",
                table: "THETIS_LOGS_RECOMENDACAO",
                type: "NVARCHAR2(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(1000)",
                oldMaxLength: 1000,
                oldNullable: true);
        }
    }
}
