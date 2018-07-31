using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IGSLControlPanel.Migrations
{
    public partial class removeIdfromInsRuleTariffLink : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_InsRuleTariffLink_Id",
                table: "InsRuleTariffLink");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "InsRuleTariffLink");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "InsRuleTariffLink",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddUniqueConstraint(
                name: "AK_InsRuleTariffLink_Id",
                table: "InsRuleTariffLink",
                column: "Id");
        }
    }
}
