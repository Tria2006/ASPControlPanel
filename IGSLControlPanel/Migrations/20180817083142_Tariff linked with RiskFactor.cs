using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IGSLControlPanel.Migrations
{
    public partial class TarifflinkedwithRiskFactor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TariffId",
                table: "RiskFactors",
                nullable: true);

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
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
