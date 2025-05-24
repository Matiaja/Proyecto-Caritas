using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoCaritas.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DonationRequestId",
                table: "OrderLines");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "OrderLines",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "AsignationDate",
                table: "DonationRequests",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "OrderLines");

            migrationBuilder.DropColumn(
                name: "AsignationDate",
                table: "DonationRequests");

            migrationBuilder.AddColumn<int>(
                name: "DonationRequestId",
                table: "OrderLines",
                type: "int",
                nullable: true);
        }
    }
}
