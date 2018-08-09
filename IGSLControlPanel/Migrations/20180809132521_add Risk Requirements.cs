using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IGSLControlPanel.Migrations
{
    public partial class addRiskRequirements : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RiskRequirements",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TariffId = table.Column<Guid>(nullable: false),
                    RiskId = table.Column<Guid>(nullable: false),
                    IsRequired = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskRequirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RiskRequirements_Risks_RiskId",
                        column: x => x.RiskId,
                        principalTable: "Risks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RiskRequirements_RiskId",
                table: "RiskRequirements",
                column: "RiskId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RiskRequirements");
        }
    }
}
