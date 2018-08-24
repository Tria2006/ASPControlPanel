using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IGSLControlPanel.Migrations
{
    public partial class removedParameterIdfromLimitListItemmodel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LimitListItems_ValueLimits_ValueLimitId",
                table: "LimitListItems");

            migrationBuilder.DropColumn(
                name: "ParameterId",
                table: "LimitListItems");

            migrationBuilder.AlterColumn<Guid>(
                name: "ValueLimitId",
                table: "LimitListItems",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LimitListItems_ValueLimits_ValueLimitId",
                table: "LimitListItems",
                column: "ValueLimitId",
                principalTable: "ValueLimits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LimitListItems_ValueLimits_ValueLimitId",
                table: "LimitListItems");

            migrationBuilder.AlterColumn<Guid>(
                name: "ValueLimitId",
                table: "LimitListItems",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AddColumn<Guid>(
                name: "ParameterId",
                table: "LimitListItems",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddForeignKey(
                name: "FK_LimitListItems_ValueLimits_ValueLimitId",
                table: "LimitListItems",
                column: "ValueLimitId",
                principalTable: "ValueLimits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
