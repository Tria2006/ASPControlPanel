using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IGSLControlPanel.Migrations
{
    public partial class addedInsuranceRulemodellinktotariff : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InsuranceRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InsuranceRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InsRuleTariffLink",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TariffId = table.Column<Guid>(nullable: false),
                    InsRuleId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InsRuleTariffLink", x => new { x.TariffId, x.InsRuleId });
                    table.UniqueConstraint("AK_InsRuleTariffLink_Id", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InsRuleTariffLink_InsuranceRules_InsRuleId",
                        column: x => x.InsRuleId,
                        principalTable: "InsuranceRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InsRuleTariffLink_Tariffs_TariffId",
                        column: x => x.TariffId,
                        principalTable: "Tariffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InsRuleTariffLink_InsRuleId",
                table: "InsRuleTariffLink",
                column: "InsRuleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InsRuleTariffLink");

            migrationBuilder.DropTable(
                name: "InsuranceRules");
        }
    }
}
