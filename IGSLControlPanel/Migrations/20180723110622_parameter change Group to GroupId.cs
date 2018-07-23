using Microsoft.EntityFrameworkCore.Migrations;

namespace IGSLControlPanel.Migrations
{
    public partial class parameterchangeGrouptoGroupId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductParameters_ParameterGroups_GroupId",
                table: "ProductParameters");

            migrationBuilder.DropIndex(
                name: "IX_ProductParameters_GroupId",
                table: "ProductParameters");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
    }
}
