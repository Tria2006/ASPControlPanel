﻿// <auto-generated />
using System;
using IGSLControlPanel.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace IGSLControlPanel.Migrations
{
    [DbContext(typeof(IGSLContext))]
    [Migration("20180817142014_Parameter group CanRepeat field")]
    partial class ParametergroupCanRepeatfield
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.1-rtm-30846")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("DBModels.Models.FactorValue", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreateDate");

                    b.Property<bool>("IsDeleted");

                    b.Property<DateTime>("ModifyDate");

                    b.Property<string>("Name");

                    b.Property<Guid>("RiskFactorId");

                    b.Property<Guid>("RiskId");

                    b.Property<Guid>("TariffId");

                    b.Property<DateTime?>("ValidFrom");

                    b.Property<DateTime?>("ValidTo");

                    b.Property<int>("Value");

                    b.HasKey("Id");

                    b.HasIndex("RiskFactorId");

                    b.ToTable("FactorValues");
                });

            modelBuilder.Entity("DBModels.Models.FolderTreeEntry", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreateDate");

                    b.Property<bool>("IsDeleted");

                    b.Property<int>("ModelTypeId");

                    b.Property<DateTime>("ModifyDate");

                    b.Property<string>("Name");

                    b.Property<Guid?>("ParentFolderId");

                    b.Property<DateTime?>("ValidFrom");

                    b.Property<DateTime?>("ValidTo");

                    b.HasKey("Id");

                    b.ToTable("FolderTreeEntries");
                });

            modelBuilder.Entity("DBModels.Models.InsuranceRule", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreateDate");

                    b.Property<bool>("IsDeleted");

                    b.Property<DateTime>("ModifyDate");

                    b.Property<string>("Name");

                    b.Property<DateTime?>("ValidFrom");

                    b.Property<DateTime?>("ValidTo");

                    b.HasKey("Id");

                    b.ToTable("InsuranceRules");
                });

            modelBuilder.Entity("DBModels.Models.ManyToManyLinks.InsRuleTariffLink", b =>
                {
                    b.Property<Guid>("TariffId");

                    b.Property<Guid>("InsRuleId");

                    b.HasKey("TariffId", "InsRuleId");

                    b.HasIndex("InsRuleId");

                    b.ToTable("InsRuleTariffLink");
                });

            modelBuilder.Entity("DBModels.Models.ManyToManyLinks.ProductLinkToProductParameter", b =>
                {
                    b.Property<Guid>("ProductId");

                    b.Property<Guid>("ProductParameterId");

                    b.HasKey("ProductId", "ProductParameterId");

                    b.HasIndex("ProductParameterId");

                    b.ToTable("ProductLinkToProductParameter");
                });

            modelBuilder.Entity("DBModels.Models.ManyToManyLinks.RiskFactorTariffLink", b =>
                {
                    b.Property<Guid>("RiskFactorId");

                    b.Property<Guid>("TariffId");

                    b.HasKey("RiskFactorId", "TariffId");

                    b.HasIndex("TariffId");

                    b.ToTable("RiskFactorTariffLink");
                });

            modelBuilder.Entity("DBModels.Models.ManyToManyLinks.RiskInsRuleLink", b =>
                {
                    b.Property<Guid>("RiskId");

                    b.Property<Guid>("InsRuleId");

                    b.HasKey("RiskId", "InsRuleId");

                    b.HasIndex("InsRuleId");

                    b.ToTable("RiskInsRuleLink");
                });

            modelBuilder.Entity("DBModels.Models.ParameterGroup", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("CanRepeat");

                    b.Property<DateTime>("CreateDate");

                    b.Property<bool>("IsDeleted");

                    b.Property<DateTime>("ModifyDate");

                    b.Property<string>("Name");

                    b.Property<DateTime?>("ValidFrom");

                    b.Property<DateTime?>("ValidTo");

                    b.HasKey("Id");

                    b.ToTable("ParameterGroups");
                });

            modelBuilder.Entity("DBModels.Models.Product", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreateDate");

                    b.Property<Guid>("FolderId");

                    b.Property<bool>("IsDeleted");

                    b.Property<DateTime>("ModifyDate");

                    b.Property<string>("Name");

                    b.Property<DateTime?>("ValidFrom");

                    b.Property<DateTime?>("ValidTo");

                    b.HasKey("Id");

                    b.ToTable("Products");
                });

            modelBuilder.Entity("DBModels.Models.ProductParameter", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreateDate");

                    b.Property<int>("DataType");

                    b.Property<Guid?>("GroupId");

                    b.Property<bool>("IsDeleted");

                    b.Property<bool>("IsRequiredForCalc");

                    b.Property<bool>("IsRequiredForSave");

                    b.Property<Guid?>("LimitId");

                    b.Property<DateTime>("ModifyDate");

                    b.Property<string>("Name");

                    b.Property<int>("Order");

                    b.Property<DateTime?>("ValidFrom");

                    b.Property<DateTime?>("ValidTo");

                    b.HasKey("Id");

                    b.HasIndex("LimitId");

                    b.ToTable("ProductParameters");
                });

            modelBuilder.Entity("DBModels.Models.Risk", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("BaseTariffType");

                    b.Property<int>("BaseTariffValue");

                    b.Property<DateTime>("CreateDate");

                    b.Property<bool>("IsDeleted");

                    b.Property<DateTime>("ModifyDate");

                    b.Property<string>("Name");

                    b.Property<DateTime?>("ValidFrom");

                    b.Property<DateTime?>("ValidTo");

                    b.HasKey("Id");

                    b.ToTable("Risks");
                });

            modelBuilder.Entity("DBModels.Models.RiskFactor", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreateDate");

                    b.Property<bool>("IsDeleted");

                    b.Property<DateTime>("ModifyDate");

                    b.Property<string>("Name");

                    b.Property<DateTime?>("ValidFrom");

                    b.Property<DateTime?>("ValidTo");

                    b.HasKey("Id");

                    b.ToTable("RiskFactors");
                });

            modelBuilder.Entity("DBModels.Models.RiskRequirement", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("IsRequired");

                    b.Property<Guid>("RiskId");

                    b.Property<Guid>("TariffId");

                    b.HasKey("Id");

                    b.HasIndex("RiskId");

                    b.ToTable("RiskRequirements");
                });

            modelBuilder.Entity("DBModels.Models.Tariff", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreateDate");

                    b.Property<Guid>("FolderId");

                    b.Property<bool>("IsDeleted");

                    b.Property<DateTime>("ModifyDate");

                    b.Property<string>("Name");

                    b.Property<DateTime?>("ValidFrom");

                    b.Property<DateTime?>("ValidTo");

                    b.HasKey("Id");

                    b.ToTable("Tariffs");
                });

            modelBuilder.Entity("DBModels.Models.ValueLimit", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreateDate");

                    b.Property<DateTime?>("DateValueFrom");

                    b.Property<DateTime?>("DateValueTo");

                    b.Property<int?>("IntValueFrom");

                    b.Property<int?>("IntValueTo");

                    b.Property<bool>("IsDeleted");

                    b.Property<DateTime>("ModifyDate");

                    b.Property<string>("Name");

                    b.Property<int>("ParameterDataType");

                    b.Property<Guid>("ParameterId");

                    b.Property<Guid>("ProductId");

                    b.Property<DateTime?>("ValidFrom");

                    b.Property<DateTime?>("ValidTo");

                    b.HasKey("Id");

                    b.ToTable("ValueLimits");
                });

            modelBuilder.Entity("DBModels.Models.FactorValue", b =>
                {
                    b.HasOne("DBModels.Models.RiskFactor")
                        .WithMany("FactorValues")
                        .HasForeignKey("RiskFactorId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DBModels.Models.ManyToManyLinks.InsRuleTariffLink", b =>
                {
                    b.HasOne("DBModels.Models.InsuranceRule", "InsRule")
                        .WithMany("LinksToTariff")
                        .HasForeignKey("InsRuleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DBModels.Models.Tariff", "Tariff")
                        .WithMany("InsRuleTariffLink")
                        .HasForeignKey("TariffId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DBModels.Models.ManyToManyLinks.ProductLinkToProductParameter", b =>
                {
                    b.HasOne("DBModels.Models.Product", "Product")
                        .WithMany("LinkToProductParameters")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DBModels.Models.ProductParameter", "Parameter")
                        .WithMany("LinkToProduct")
                        .HasForeignKey("ProductParameterId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DBModels.Models.ManyToManyLinks.RiskFactorTariffLink", b =>
                {
                    b.HasOne("DBModels.Models.RiskFactor", "RiskFactor")
                        .WithMany("RiskFactorsTariffLinks")
                        .HasForeignKey("RiskFactorId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DBModels.Models.Tariff", "Tariff")
                        .WithMany("RiskFactorsTariffLinks")
                        .HasForeignKey("TariffId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DBModels.Models.ManyToManyLinks.RiskInsRuleLink", b =>
                {
                    b.HasOne("DBModels.Models.InsuranceRule", "InsRule")
                        .WithMany("LinksToRisks")
                        .HasForeignKey("InsRuleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DBModels.Models.Risk", "Risk")
                        .WithMany("LinksToInsRules")
                        .HasForeignKey("RiskId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DBModels.Models.ProductParameter", b =>
                {
                    b.HasOne("DBModels.Models.ValueLimit", "Limit")
                        .WithMany()
                        .HasForeignKey("LimitId");
                });

            modelBuilder.Entity("DBModels.Models.RiskRequirement", b =>
                {
                    b.HasOne("DBModels.Models.Risk")
                        .WithMany("Requirements")
                        .HasForeignKey("RiskId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
