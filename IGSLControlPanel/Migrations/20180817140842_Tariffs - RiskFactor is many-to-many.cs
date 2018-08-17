using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IGSLControlPanel.Migrations
{
    public partial class TariffsRiskFactorismanytomany : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RiskFactors_Tariffs_TariffId",
                table: "RiskFactors");

            migrationBuilder.DropIndex(
                name: "IX_RiskFactors_TariffId",
                table: "RiskFactors");

            migrationBuilder.DropColumn(
                name: "TariffId",
                table: "RiskFactors");

            migrationBuilder.CreateTable(
                name: "RiskFactorTariffLink",
                columns: table => new
                {
                    RiskFactorId = table.Column<Guid>(nullable: false),
                    TariffId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskFactorTariffLink", x => new { x.RiskFactorId, x.TariffId });
                    table.ForeignKey(
                        name: "FK_RiskFactorTariffLink_RiskFactors_RiskFactorId",
                        column: x => x.RiskFactorId,
                        principalTable: "RiskFactors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RiskFactorTariffLink_Tariffs_TariffId",
                        column: x => x.TariffId,
                        principalTable: "Tariffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RiskFactorTariffLink_TariffId",
                table: "RiskFactorTariffLink",
                column: "TariffId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RiskFactorTariffLink");

            migrationBuilder.AddColumn<Guid>(
                name: "TariffId",
                table: "RiskFactors",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_RiskFactors_TariffId",
                table: "RiskFactors",
                column: "TariffId");

            migrationBuilder.AddForeignKey(
                name: "FK_RiskFactors_Tariffs_TariffId",
                table: "RiskFactors",
                column: "TariffId",
                principalTable: "Tariffs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
