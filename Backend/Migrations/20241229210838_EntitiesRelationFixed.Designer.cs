﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProyectoCaritas.Data;

#nullable disable

namespace ProyectoCaritas.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20241229210838_EntitiesRelationFixed")]
    partial class EntitiesRelationFixed
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("ProyectoCaritas.Models.Entities.Category", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("ProyectoCaritas.Models.Entities.Center", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("CapacityLimit")
                        .HasColumnType("int");

                    b.Property<string>("Email")
                        .HasColumnType("longtext");

                    b.Property<string>("Location")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Manager")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Centers");
                });

            modelBuilder.Entity("ProyectoCaritas.Models.Entities.DonationRequest", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("AssignedCenterId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("ReceptionDate")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("ShipmentDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("AssignedCenterId");

                    b.ToTable("DonationRequests");
                });

            modelBuilder.Entity("ProyectoCaritas.Models.Entities.OrderLine", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .HasColumnType("longtext");

                    b.Property<int?>("DonationRequestId")
                        .HasColumnType("int");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.Property<int?>("RequestId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("DonationRequestId");

                    b.HasIndex("RequestId");

                    b.ToTable("OrderLines");
                });

            modelBuilder.Entity("ProyectoCaritas.Models.Entities.Product", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("CategoryId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("ExpirationDate")
                        .HasColumnType("datetime(6)");

                    b.Property<byte[]>("Image")
                        .HasColumnType("longblob");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int?>("OrderLineId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.HasIndex("OrderLineId");

                    b.ToTable("Products");
                });

            modelBuilder.Entity("ProyectoCaritas.Models.Entities.Request", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("RequestDate")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("RequestingCenterId")
                        .HasColumnType("int");

                    b.Property<string>("UrgencyLevel")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("RequestingCenterId");

                    b.ToTable("Requests");
                });

            modelBuilder.Entity("ProyectoCaritas.Models.Entities.Stock", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("CenterId")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .HasColumnType("longtext");

                    b.Property<DateTime>("EntryDate")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime?>("ExpirationDate")
                        .HasColumnType("datetime(6)");

                    b.Property<int?>("ProductId")
                        .HasColumnType("int");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<double>("Weight")
                        .HasColumnType("double");

                    b.HasKey("Id");

                    b.HasIndex("CenterId");

                    b.HasIndex("ProductId");

                    b.ToTable("Stocks");
                });

            modelBuilder.Entity("ProyectoCaritas.Models.Entities.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("CenterId")
                        .HasColumnType("int");

                    b.Property<string>("Email")
                        .HasColumnType("longtext");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int?>("StorageCenterId")
                        .HasColumnType("int");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("CenterId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("RequestUser", b =>
                {
                    b.Property<int>("RequestsId")
                        .HasColumnType("int");

                    b.Property<int>("UsersId")
                        .HasColumnType("int");

                    b.HasKey("RequestsId", "UsersId");

                    b.HasIndex("UsersId");

                    b.ToTable("RequestUser");
                });

            modelBuilder.Entity("ProyectoCaritas.Models.Entities.DonationRequest", b =>
                {
                    b.HasOne("ProyectoCaritas.Models.Entities.Center", "AssignedCenter")
                        .WithMany("DonationRequests")
                        .HasForeignKey("AssignedCenterId");

                    b.Navigation("AssignedCenter");
                });

            modelBuilder.Entity("ProyectoCaritas.Models.Entities.OrderLine", b =>
                {
                    b.HasOne("ProyectoCaritas.Models.Entities.DonationRequest", "DonationRequest")
                        .WithMany("OrderLines")
                        .HasForeignKey("DonationRequestId");

                    b.HasOne("ProyectoCaritas.Models.Entities.Request", "Request")
                        .WithMany("OrderLines")
                        .HasForeignKey("RequestId");

                    b.Navigation("DonationRequest");

                    b.Navigation("Request");
                });

            modelBuilder.Entity("ProyectoCaritas.Models.Entities.Product", b =>
                {
                    b.HasOne("ProyectoCaritas.Models.Entities.Category", "Category")
                        .WithMany("Products")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ProyectoCaritas.Models.Entities.OrderLine", "OrderLine")
                        .WithMany("Products")
                        .HasForeignKey("OrderLineId");

                    b.Navigation("Category");

                    b.Navigation("OrderLine");
                });

            modelBuilder.Entity("ProyectoCaritas.Models.Entities.Request", b =>
                {
                    b.HasOne("ProyectoCaritas.Models.Entities.Center", "RequestingCenter")
                        .WithMany()
                        .HasForeignKey("RequestingCenterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("RequestingCenter");
                });

            modelBuilder.Entity("ProyectoCaritas.Models.Entities.Stock", b =>
                {
                    b.HasOne("ProyectoCaritas.Models.Entities.Center", "Center")
                        .WithMany("Stocks")
                        .HasForeignKey("CenterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ProyectoCaritas.Models.Entities.Product", "Product")
                        .WithMany("Stocks")
                        .HasForeignKey("ProductId");

                    b.Navigation("Center");

                    b.Navigation("Product");
                });

            modelBuilder.Entity("ProyectoCaritas.Models.Entities.User", b =>
                {
                    b.HasOne("ProyectoCaritas.Models.Entities.Center", "Center")
                        .WithMany("Users")
                        .HasForeignKey("CenterId");

                    b.Navigation("Center");
                });

            modelBuilder.Entity("RequestUser", b =>
                {
                    b.HasOne("ProyectoCaritas.Models.Entities.Request", null)
                        .WithMany()
                        .HasForeignKey("RequestsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ProyectoCaritas.Models.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UsersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ProyectoCaritas.Models.Entities.Category", b =>
                {
                    b.Navigation("Products");
                });

            modelBuilder.Entity("ProyectoCaritas.Models.Entities.Center", b =>
                {
                    b.Navigation("DonationRequests");

                    b.Navigation("Stocks");

                    b.Navigation("Users");
                });

            modelBuilder.Entity("ProyectoCaritas.Models.Entities.DonationRequest", b =>
                {
                    b.Navigation("OrderLines");
                });

            modelBuilder.Entity("ProyectoCaritas.Models.Entities.OrderLine", b =>
                {
                    b.Navigation("Products");
                });

            modelBuilder.Entity("ProyectoCaritas.Models.Entities.Product", b =>
                {
                    b.Navigation("Stocks");
                });

            modelBuilder.Entity("ProyectoCaritas.Models.Entities.Request", b =>
                {
                    b.Navigation("OrderLines");
                });
#pragma warning restore 612, 618
        }
    }
}
