using Microsoft.EntityFrameworkCore.Migrations;

namespace IGSLControlPanel.Migrations
{
    public partial class BaseTariffreturntoRiskfromTariff : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaseTariffType",
                table: "Tariffs");

            migrationBuilder.DropColumn(
                name: "BaseTariffValue",
                table: "Tariffs");

            migrationBuilder.AddColumn<int>(
                name: "BaseTariffType",
                table: "Risks",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "BaseTariffValue",
                table: "Risks",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaseTariffType",
                table: "Risks");

            migrationBuilder.DropColumn(
                name: "BaseTariffValue",
                table: "Risks");

            migrationBuilder.AddColumn<int>(
                name: "BaseTariffType",
                table: "Tariffs",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "BaseTariffValue",
                table: "Tariffs",
                nullable: true);
        }
    }
}
