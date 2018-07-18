using Microsoft.EntityFrameworkCore.Migrations;

namespace IGSLControlPanel.Migrations
{
    public partial class Paramsadd2boolfieldsRequired : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRequiredForCalc",
                table: "ProductParameter",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRequiredForSave",
                table: "ProductParameter",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRequiredForCalc",
                table: "ProductParameter");

            migrationBuilder.DropColumn(
                name: "IsRequiredForSave",
                table: "ProductParameter");
        }
    }
}
