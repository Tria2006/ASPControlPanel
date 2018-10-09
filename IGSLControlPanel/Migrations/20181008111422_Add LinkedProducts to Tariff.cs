using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IGSLControlPanel.Migrations
{
    public partial class AddLinkedProductstoTariff : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TariffId",
                table: "Products",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_TariffId",
                table: "Products",
                column: "TariffId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Tariffs_TariffId",
                table: "Products",
                column: "TariffId",
                principalTable: "Tariffs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Tariffs_TariffId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_TariffId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "TariffId",
                table: "Products");
        }
    }
}
