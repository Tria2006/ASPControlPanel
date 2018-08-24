using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IGSLControlPanel.Migrations
{
    public partial class addedLimitListItemmodel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LimitListItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: false),
                    ValidFrom = table.Column<DateTime>(nullable: true),
                    ValidTo = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Value = table.Column<string>(nullable: true),
                    ParameterId = table.Column<Guid>(nullable: false),
                    ValueLimitId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LimitListItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LimitListItems_ValueLimits_ValueLimitId",
                        column: x => x.ValueLimitId,
                        principalTable: "ValueLimits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LimitListItems_ValueLimitId",
                table: "LimitListItems",
                column: "ValueLimitId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LimitListItems");
        }
    }
}
