using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IGSLControlPanel.Migrations
{
    public partial class ProducttoProductParametermanytomanyrelation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductParameter",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductParameter", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductLinkToProductParameter",
                columns: table => new
                {
                    ProductId = table.Column<Guid>(nullable: false),
                    ProductParameterId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductLinkToProductParameter", x => new { x.ProductId, x.ProductParameterId });
                    table.ForeignKey(
                        name: "FK_ProductLinkToProductParameter_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductLinkToProductParameter_ProductParameter_ProductParameterId",
                        column: x => x.ProductParameterId,
                        principalTable: "ProductParameter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductLinkToProductParameter_ProductParameterId",
                table: "ProductLinkToProductParameter",
                column: "ProductParameterId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductLinkToProductParameter");

            migrationBuilder.DropTable(
                name: "ProductParameter");
        }
    }
}
