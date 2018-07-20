using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IGSLControlPanel.Migrations
{
    public partial class addedValueLimitmodel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LimitId",
                table: "ProductParameters",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ValueLimits",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    ParameterDataType = table.Column<int>(nullable: false),
                    IntValueFrom = table.Column<int>(nullable: true),
                    IntValueTo = table.Column<int>(nullable: true),
                    DateValueFrom = table.Column<DateTime>(nullable: true),
                    DateValueTo = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    ParameterId = table.Column<Guid>(nullable: false),
                    ProductId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValueLimits", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductParameters_LimitId",
                table: "ProductParameters",
                column: "LimitId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductParameters_ValueLimits_LimitId",
                table: "ProductParameters",
                column: "LimitId",
                principalTable: "ValueLimits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductParameters_ValueLimits_LimitId",
                table: "ProductParameters");

            migrationBuilder.DropTable(
                name: "ValueLimits");

            migrationBuilder.DropIndex(
                name: "IX_ProductParameters_LimitId",
                table: "ProductParameters");

            migrationBuilder.DropColumn(
                name: "LimitId",
                table: "ProductParameters");
        }
    }
}
