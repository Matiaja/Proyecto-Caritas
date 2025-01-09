using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProyectoCaritas.Migrations
{
    /// <inheritdoc />
    public partial class AlejandroMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderLines_DonationRequests_DonationRequestId",
                table: "OrderLines");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_OrderLines_OrderLineId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_OrderLineId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_OrderLines_DonationRequestId",
                table: "OrderLines");

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "DonationRequests",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "DonationRequests",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Centers",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Centers",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DropColumn(
                name: "ExpirationDate",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Image",
                table: "Products");

            migrationBuilder.AddColumn<byte[]>(
                name: "Image",
                table: "Stocks",
                type: "longblob",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "OrderLines",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrderLineId",
                table: "DonationRequests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderLines_DonationRequestId",
                table: "OrderLines",
                column: "DonationRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderLines_ProductId",
                table: "OrderLines",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLines_DonationRequests_DonationRequestId",
                table: "OrderLines",
                column: "DonationRequestId",
                principalTable: "DonationRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLines_Products_ProductId",
                table: "OrderLines",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderLines_DonationRequests_DonationRequestId",
                table: "OrderLines");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderLines_Products_ProductId",
                table: "OrderLines");

            migrationBuilder.DropIndex(
                name: "IX_OrderLines_DonationRequestId",
                table: "OrderLines");

            migrationBuilder.DropIndex(
                name: "IX_OrderLines_ProductId",
                table: "OrderLines");

            migrationBuilder.DropColumn(
                name: "Image",
                table: "Stocks");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "OrderLines");

            migrationBuilder.DropColumn(
                name: "OrderLineId",
                table: "DonationRequests");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpirationDate",
                table: "Products",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Image",
                table: "Products",
                type: "longblob",
                nullable: true);

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, null, "Alimentos" },
                    { 2, null, "Ropa" },
                    { 3, null, "Calzado" },
                    { 4, null, "Juguetes" },
                    { 5, null, "Material escolar" }
                });

            migrationBuilder.InsertData(
                table: "Centers",
                columns: new[] { "Id", "CapacityLimit", "Email", "Location", "Manager", "Name", "Phone" },
                values: new object[,]
                {
                    { 1, 100, null, "Calle de la Caridad, 1", "Juan Pérez", "Centro Caritas 1", "123456789" },
                    { 2, 150, null, "Calle de Dios, 2", "María López", "Centro Caritas 2", "987654321" }
                });

            migrationBuilder.InsertData(
                table: "DonationRequests",
                columns: new[] { "Id", "AssignedCenterId", "ReceptionDate", "ShipmentDate", "Status" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2025, 1, 2, 16, 42, 33, 236, DateTimeKind.Local).AddTicks(6704), new DateTime(2025, 1, 2, 16, 42, 33, 236, DateTimeKind.Local).AddTicks(6689), "Recibido" },
                    { 2, 2, new DateTime(2025, 1, 2, 16, 42, 33, 236, DateTimeKind.Local).AddTicks(6709), new DateTime(2025, 1, 2, 16, 42, 33, 236, DateTimeKind.Local).AddTicks(6708), "Recibido" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_OrderLineId",
                table: "Products",
                column: "OrderLineId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLines_DonationRequestId",
                table: "OrderLines",
                column: "DonationRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLines_DonationRequests_DonationRequestId",
                table: "OrderLines",
                column: "DonationRequestId",
                principalTable: "DonationRequests",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_OrderLines_OrderLineId",
                table: "Products",
                column: "OrderLineId",
                principalTable: "OrderLines",
                principalColumn: "Id");
        }
    }
}
