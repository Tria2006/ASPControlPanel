﻿// <auto-generated />
using System;
using IGSLControlPanel.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace IGSLControlPanel.Migrations
{
    [DbContext(typeof(IGSLContext))]
    partial class IGSLContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.1-rtm-30846")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("IGSLControlPanel.Models.FolderTreeEntry", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("IsDeleted");

                    b.Property<string>("Name");

                    b.Property<Guid?>("ParentFolderId");

                    b.HasKey("Id");

                    b.ToTable("FolderTreeEntries");
                });

            modelBuilder.Entity("IGSLControlPanel.Models.ManyToManyLinks.ProductLinkToProductParameter", b =>
                {
                    b.Property<Guid>("ProductId");

                    b.Property<Guid>("ProductParameterId");

                    b.HasKey("ProductId", "ProductParameterId");

                    b.HasIndex("ProductParameterId");

                    b.ToTable("ProductLinkToProductParameter");
                });

            modelBuilder.Entity("IGSLControlPanel.Models.ParameterGroup", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("IsDeleted");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("ParameterGroups");
                });

            modelBuilder.Entity("IGSLControlPanel.Models.Product", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid?>("FolderId");

                    b.Property<bool>("IsDeleted");

                    b.Property<string>("Name");

                    b.Property<DateTime?>("ValidFrom");

                    b.Property<DateTime?>("ValidTo");

                    b.HasKey("Id");

                    b.ToTable("Products");
                });

            modelBuilder.Entity("IGSLControlPanel.Models.ProductParameter", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("DataType");

                    b.Property<Guid?>("GroupId");

                    b.Property<bool>("IsDeleted");

                    b.Property<bool>("IsRequiredForCalc");

                    b.Property<bool>("IsRequiredForSave");

                    b.Property<Guid?>("LimitId");

                    b.Property<string>("Name");

                    b.Property<int>("Order");

                    b.HasKey("Id");

                    b.HasIndex("LimitId");

                    b.ToTable("ProductParameters");
                });

            modelBuilder.Entity("IGSLControlPanel.Models.Tariff", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid?>("FolderId");

                    b.Property<bool>("IsDeleted");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Tariffs");
                });

            modelBuilder.Entity("IGSLControlPanel.Models.ValueLimit", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime?>("DateValueFrom");

                    b.Property<DateTime?>("DateValueTo");

                    b.Property<int?>("IntValueFrom");

                    b.Property<int?>("IntValueTo");

                    b.Property<bool>("IsDeleted");

                    b.Property<string>("Name");

                    b.Property<int>("ParameterDataType");

                    b.Property<Guid>("ParameterId");

                    b.Property<Guid>("ProductId");

                    b.HasKey("Id");

                    b.ToTable("ValueLimits");
                });

            modelBuilder.Entity("IGSLControlPanel.Models.ManyToManyLinks.ProductLinkToProductParameter", b =>
                {
                    b.HasOne("IGSLControlPanel.Models.Product", "Product")
                        .WithMany("LinkToProductParameters")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("IGSLControlPanel.Models.ProductParameter", "Parameter")
                        .WithMany("LinkToProduct")
                        .HasForeignKey("ProductParameterId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("IGSLControlPanel.Models.ProductParameter", b =>
                {
                    b.HasOne("IGSLControlPanel.Models.ValueLimit", "Limit")
                        .WithMany()
                        .HasForeignKey("LimitId");
                });
#pragma warning restore 612, 618
        }
    }
}
