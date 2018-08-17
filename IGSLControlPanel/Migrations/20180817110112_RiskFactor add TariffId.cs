using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IGSLControlPanel.Migrations
{
    public partial class RiskFactoraddTariffId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RiskFactors_Tariffs_TariffId",
                table: "RiskFactors");

            migrationBuilder.AlterColumn<Guid>(
                name: "TariffId",
                table: "RiskFactors",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RiskFactors_Tariffs_TariffId",
                table: "RiskFactors",
                column: "TariffId",
                principalTable: "Tariffs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RiskFactors_Tariffs_TariffId",
                table: "RiskFactors");

            migrationBuilder.AlterColumn<Guid>(
                name: "TariffId",
                table: "RiskFactors",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AddForeignKey(
                name: "FK_RiskFactors_Tariffs_TariffId",
                table: "RiskFactors",
                column: "TariffId",
                principalTable: "Tariffs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
