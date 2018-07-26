using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IGSLControlPanel.Migrations
{
    public partial class addModelTypeIdtoFolders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ValidFrom",
                table: "Tariffs",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidTo",
                table: "Tariffs",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ModelTypeId",
                table: "FolderTreeEntries",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ValidFrom",
                table: "Tariffs");

            migrationBuilder.DropColumn(
                name: "ValidTo",
                table: "Tariffs");

            migrationBuilder.DropColumn(
                name: "ModelTypeId",
                table: "FolderTreeEntries");
        }
    }
}
