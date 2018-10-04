using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IGSLControlPanel.Migrations
{
    public partial class AddIsConstantandValuestoParametermodel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ConstantValueDate",
                table: "ProductParameters",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConstantValueInt",
                table: "ProductParameters",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConstantValueStr",
                table: "ProductParameters",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsConstant",
                table: "ProductParameters",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConstantValueDate",
                table: "ProductParameters");

            migrationBuilder.DropColumn(
                name: "ConstantValueInt",
                table: "ProductParameters");

            migrationBuilder.DropColumn(
                name: "ConstantValueStr",
                table: "ProductParameters");

            migrationBuilder.DropColumn(
                name: "IsConstant",
                table: "ProductParameters");
        }
    }
}
