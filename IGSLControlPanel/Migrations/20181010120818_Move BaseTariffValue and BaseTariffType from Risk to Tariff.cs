using Microsoft.EntityFrameworkCore.Migrations;

namespace IGSLControlPanel.Migrations
{
    public partial class MoveBaseTariffValueandBaseTariffTypefromRisktoTariff : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "BaseTariffValue",
                table: "Tariffs",
                nullable: false,
                defaultValue: 1);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<int>(
                name: "BaseTariffValue",
                table: "Risks",
                nullable: false,
                defaultValue: 0);
        }
    }
}
