using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IGSLControlPanel.Migrations
{
    public partial class addBaseModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "ValueLimits",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifyDate",
                table: "ValueLimits",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidFrom",
                table: "ValueLimits",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidTo",
                table: "ValueLimits",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "Tariffs",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifyDate",
                table: "Tariffs",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "Products",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifyDate",
                table: "Products",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "ProductParameters",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifyDate",
                table: "ProductParameters",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidFrom",
                table: "ProductParameters",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidTo",
                table: "ProductParameters",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "ParameterGroups",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifyDate",
                table: "ParameterGroups",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidFrom",
                table: "ParameterGroups",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidTo",
                table: "ParameterGroups",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "InsuranceRules",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifyDate",
                table: "InsuranceRules",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidFrom",
                table: "InsuranceRules",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidTo",
                table: "InsuranceRules",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "FolderTreeEntries",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifyDate",
                table: "FolderTreeEntries",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidFrom",
                table: "FolderTreeEntries",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidTo",
                table: "FolderTreeEntries",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "ValueLimits");

            migrationBuilder.DropColumn(
                name: "ModifyDate",
                table: "ValueLimits");

            migrationBuilder.DropColumn(
                name: "ValidFrom",
                table: "ValueLimits");

            migrationBuilder.DropColumn(
                name: "ValidTo",
                table: "ValueLimits");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "Tariffs");

            migrationBuilder.DropColumn(
                name: "ModifyDate",
                table: "Tariffs");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ModifyDate",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "ProductParameters");

            migrationBuilder.DropColumn(
                name: "ModifyDate",
                table: "ProductParameters");

            migrationBuilder.DropColumn(
                name: "ValidFrom",
                table: "ProductParameters");

            migrationBuilder.DropColumn(
                name: "ValidTo",
                table: "ProductParameters");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "ParameterGroups");

            migrationBuilder.DropColumn(
                name: "ModifyDate",
                table: "ParameterGroups");

            migrationBuilder.DropColumn(
                name: "ValidFrom",
                table: "ParameterGroups");

            migrationBuilder.DropColumn(
                name: "ValidTo",
                table: "ParameterGroups");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "InsuranceRules");

            migrationBuilder.DropColumn(
                name: "ModifyDate",
                table: "InsuranceRules");

            migrationBuilder.DropColumn(
                name: "ValidFrom",
                table: "InsuranceRules");

            migrationBuilder.DropColumn(
                name: "ValidTo",
                table: "InsuranceRules");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "FolderTreeEntries");

            migrationBuilder.DropColumn(
                name: "ModifyDate",
                table: "FolderTreeEntries");

            migrationBuilder.DropColumn(
                name: "ValidFrom",
                table: "FolderTreeEntries");

            migrationBuilder.DropColumn(
                name: "ValidTo",
                table: "FolderTreeEntries");
        }
    }
}
