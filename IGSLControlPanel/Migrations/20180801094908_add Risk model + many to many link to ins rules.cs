using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IGSLControlPanel.Migrations
{
    public partial class addRiskmodelmanytomanylinktoinsrules : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Risks",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: false),
                    ValidFrom = table.Column<DateTime>(nullable: true),
                    ValidTo = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    BaseTariffValue = table.Column<int>(nullable: false),
                    BaseTariffType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Risks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RiskInsRuleLink",
                columns: table => new
                {
                    RiskId = table.Column<Guid>(nullable: false),
                    InsRuleId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskInsRuleLink", x => new { x.RiskId, x.InsRuleId });
                    table.ForeignKey(
                        name: "FK_RiskInsRuleLink_InsuranceRules_InsRuleId",
                        column: x => x.InsRuleId,
                        principalTable: "InsuranceRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RiskInsRuleLink_Risks_RiskId",
                        column: x => x.RiskId,
                        principalTable: "Risks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RiskInsRuleLink_InsRuleId",
                table: "RiskInsRuleLink",
                column: "InsRuleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RiskInsRuleLink");

            migrationBuilder.DropTable(
                name: "Risks");
        }
    }
}
