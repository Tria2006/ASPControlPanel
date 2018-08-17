using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IGSLControlPanel.Migrations
{
    public partial class FactorValueaddRiskFactorId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FactorValues_RiskFactors_RiskFactorId",
                table: "FactorValues");

            migrationBuilder.AlterColumn<Guid>(
                name: "RiskFactorId",
                table: "FactorValues",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FactorValues_RiskFactors_RiskFactorId",
                table: "FactorValues",
                column: "RiskFactorId",
                principalTable: "RiskFactors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FactorValues_RiskFactors_RiskFactorId",
                table: "FactorValues");

            migrationBuilder.AlterColumn<Guid>(
                name: "RiskFactorId",
                table: "FactorValues",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AddForeignKey(
                name: "FK_FactorValues_RiskFactors_RiskFactorId",
                table: "FactorValues",
                column: "RiskFactorId",
                principalTable: "RiskFactors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
