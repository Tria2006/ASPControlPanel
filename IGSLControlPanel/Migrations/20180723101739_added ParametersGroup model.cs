using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IGSLControlPanel.Migrations
{
    public partial class addedParametersGroupmodel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GroupId",
                table: "ProductParameters",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ParameterGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParameterGroups", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductParameters_GroupId",
                table: "ProductParameters",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductParameters_ParameterGroups_GroupId",
                table: "ProductParameters",
                column: "GroupId",
                principalTable: "ParameterGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductParameters_ParameterGroups_GroupId",
                table: "ProductParameters");

            migrationBuilder.DropTable(
                name: "ParameterGroups");

            migrationBuilder.DropIndex(
                name: "IX_ProductParameters_GroupId",
                table: "ProductParameters");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "ProductParameters");
        }
    }
}
