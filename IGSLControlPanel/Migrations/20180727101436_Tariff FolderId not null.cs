using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IGSLControlPanel.Migrations
{
    public partial class TariffFolderIdnotnull : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "FolderId",
                table: "Tariffs",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "FolderId",
                table: "Tariffs",
                nullable: true,
                oldClrType: typeof(Guid));
        }
    }
}
