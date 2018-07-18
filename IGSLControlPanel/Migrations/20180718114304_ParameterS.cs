using Microsoft.EntityFrameworkCore.Migrations;

namespace IGSLControlPanel.Migrations
{
    public partial class ParameterS : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductLinkToProductParameter_ProductParameter_ProductParameterId",
                table: "ProductLinkToProductParameter");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductParameter",
                table: "ProductParameter");

            migrationBuilder.RenameTable(
                name: "ProductParameter",
                newName: "ProductParameters");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductParameters",
                table: "ProductParameters",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductLinkToProductParameter_ProductParameters_ProductParameterId",
                table: "ProductLinkToProductParameter",
                column: "ProductParameterId",
                principalTable: "ProductParameters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductLinkToProductParameter_ProductParameters_ProductParameterId",
                table: "ProductLinkToProductParameter");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductParameters",
                table: "ProductParameters");

            migrationBuilder.RenameTable(
                name: "ProductParameters",
                newName: "ProductParameter");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductParameter",
                table: "ProductParameter",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductLinkToProductParameter_ProductParameter_ProductParameterId",
                table: "ProductLinkToProductParameter",
                column: "ProductParameterId",
                principalTable: "ProductParameter",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
