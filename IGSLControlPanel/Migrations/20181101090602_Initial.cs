using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IGSLControlPanel.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FolderTreeEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: false),
                    ValidFrom = table.Column<DateTime>(nullable: true),
                    ValidTo = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    ParentFolderId = table.Column<Guid>(nullable: true),
                    ModelTypeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FolderTreeEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InsuranceRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: false),
                    ValidFrom = table.Column<DateTime>(nullable: true),
                    ValidTo = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InsuranceRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParameterGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: false),
                    ValidFrom = table.Column<DateTime>(nullable: true),
                    ValidTo = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    CanRepeat = table.Column<bool>(nullable: false),
                    IsGlobal = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParameterGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParameterToFactorLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: false),
                    ValidFrom = table.Column<DateTime>(nullable: true),
                    ValidTo = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    ProductId = table.Column<Guid>(nullable: false),
                    TariffId = table.Column<Guid>(nullable: false),
                    ProductParameterId = table.Column<Guid>(nullable: false),
                    RiskFactorId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParameterToFactorLinks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RiskFactors",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: false),
                    ValidFrom = table.Column<DateTime>(nullable: true),
                    ValidTo = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskFactors", x => x.Id);
                });

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
                    IsDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Risks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tariffs",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: false),
                    ValidFrom = table.Column<DateTime>(nullable: true),
                    ValidTo = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    FolderId = table.Column<Guid>(nullable: false),
                    BaseTariffValue = table.Column<double>(nullable: true),
                    BaseTariffType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tariffs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ValueLimits",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: false),
                    ValidFrom = table.Column<DateTime>(nullable: true),
                    ValidTo = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    ParameterDataType = table.Column<int>(nullable: false),
                    StringValue = table.Column<string>(nullable: true),
                    IntValueFrom = table.Column<int>(nullable: true),
                    IntValueTo = table.Column<int>(nullable: true),
                    DateValueFrom = table.Column<DateTime>(nullable: true),
                    DateValueTo = table.Column<DateTime>(nullable: true),
                    ParameterId = table.Column<Guid>(nullable: false),
                    ProductId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValueLimits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FactorValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: false),
                    ValidFrom = table.Column<DateTime>(nullable: true),
                    ValidTo = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    TariffId = table.Column<Guid>(nullable: false),
                    RiskId = table.Column<Guid>(nullable: false),
                    RiskFactorId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactorValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FactorValues_RiskFactors_RiskFactorId",
                        column: x => x.RiskFactorId,
                        principalTable: "RiskFactors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateTable(
                name: "InsRuleTariffLink",
                columns: table => new
                {
                    TariffId = table.Column<Guid>(nullable: false),
                    InsRuleId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InsRuleTariffLink", x => new { x.TariffId, x.InsRuleId });
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

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: false),
                    ValidFrom = table.Column<DateTime>(nullable: true),
                    ValidTo = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    FolderId = table.Column<Guid>(nullable: false),
                    TariffId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Tariffs_TariffId",
                        column: x => x.TariffId,
                        principalTable: "Tariffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RiskFactorTariffLink",
                columns: table => new
                {
                    RiskFactorId = table.Column<Guid>(nullable: false),
                    TariffId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskFactorTariffLink", x => new { x.RiskFactorId, x.TariffId });
                    table.ForeignKey(
                        name: "FK_RiskFactorTariffLink_RiskFactors_RiskFactorId",
                        column: x => x.RiskFactorId,
                        principalTable: "RiskFactors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RiskFactorTariffLink_Tariffs_TariffId",
                        column: x => x.TariffId,
                        principalTable: "Tariffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                    ValueLimitId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LimitListItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LimitListItems_ValueLimits_ValueLimitId",
                        column: x => x.ValueLimitId,
                        principalTable: "ValueLimits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductParameters",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: false),
                    ValidFrom = table.Column<DateTime>(nullable: true),
                    ValidTo = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    DataType = table.Column<int>(nullable: false),
                    IsRequiredForCalc = table.Column<bool>(nullable: false),
                    IsRequiredForSave = table.Column<bool>(nullable: false),
                    IsConstant = table.Column<bool>(nullable: false),
                    ConstantValueStr = table.Column<string>(nullable: true),
                    ConstantValueInt = table.Column<int>(nullable: true),
                    ConstantValueDate = table.Column<DateTime>(nullable: true),
                    Order = table.Column<int>(nullable: false),
                    LimitId = table.Column<Guid>(nullable: true),
                    GroupId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductParameters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductParameters_ValueLimits_LimitId",
                        column: x => x.LimitId,
                        principalTable: "ValueLimits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Coefficients",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: false),
                    ValidFrom = table.Column<DateTime>(nullable: true),
                    ValidTo = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    RiskId = table.Column<Guid>(nullable: false),
                    FactorValueId = table.Column<Guid>(nullable: false),
                    Value = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coefficients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Coefficients_FactorValues_FactorValueId",
                        column: x => x.FactorValueId,
                        principalTable: "FactorValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                        name: "FK_ProductLinkToProductParameter_ProductParameters_ProductParameterId",
                        column: x => x.ProductParameterId,
                        principalTable: "ProductParameters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Coefficients_FactorValueId",
                table: "Coefficients",
                column: "FactorValueId");

            migrationBuilder.CreateIndex(
                name: "IX_FactorValues_RiskFactorId",
                table: "FactorValues",
                column: "RiskFactorId");

            migrationBuilder.CreateIndex(
                name: "IX_InsRuleTariffLink_InsRuleId",
                table: "InsRuleTariffLink",
                column: "InsRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_LimitListItems_ValueLimitId",
                table: "LimitListItems",
                column: "ValueLimitId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductLinkToProductParameter_ProductParameterId",
                table: "ProductLinkToProductParameter",
                column: "ProductParameterId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductParameters_LimitId",
                table: "ProductParameters",
                column: "LimitId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_TariffId",
                table: "Products",
                column: "TariffId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskFactorTariffLink_TariffId",
                table: "RiskFactorTariffLink",
                column: "TariffId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskInsRuleLink_InsRuleId",
                table: "RiskInsRuleLink",
                column: "InsRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskRequirements_RiskId",
                table: "RiskRequirements",
                column: "RiskId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Coefficients");

            migrationBuilder.DropTable(
                name: "FolderTreeEntries");

            migrationBuilder.DropTable(
                name: "InsRuleTariffLink");

            migrationBuilder.DropTable(
                name: "LimitListItems");

            migrationBuilder.DropTable(
                name: "ParameterGroups");

            migrationBuilder.DropTable(
                name: "ParameterToFactorLinks");

            migrationBuilder.DropTable(
                name: "ProductLinkToProductParameter");

            migrationBuilder.DropTable(
                name: "RiskFactorTariffLink");

            migrationBuilder.DropTable(
                name: "RiskInsRuleLink");

            migrationBuilder.DropTable(
                name: "RiskRequirements");

            migrationBuilder.DropTable(
                name: "FactorValues");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "ProductParameters");

            migrationBuilder.DropTable(
                name: "InsuranceRules");

            migrationBuilder.DropTable(
                name: "Risks");

            migrationBuilder.DropTable(
                name: "RiskFactors");

            migrationBuilder.DropTable(
                name: "Tariffs");

            migrationBuilder.DropTable(
                name: "ValueLimits");
        }
    }
}
