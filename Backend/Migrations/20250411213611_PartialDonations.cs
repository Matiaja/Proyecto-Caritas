using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoCaritas.Migrations
{
    /// <inheritdoc />
    public partial class PartialDonations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DonationRequests_Centers_AssignedCenterId",
                table: "DonationRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderLines_DonationRequests_DonationRequestId",
                table: "OrderLines");

            migrationBuilder.DropIndex(
                name: "IX_OrderLines_DonationRequestId",
                table: "OrderLines");

            migrationBuilder.AlterColumn<double>(
                name: "Weight",
                table: "Stocks",
                type: "double",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ShipmentDate",
                table: "DonationRequests",
                type: "datetime(6)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AlterColumn<int>(
                name: "OrderLineId",
                table: "DonationRequests",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AssignedCenterId",
                table: "DonationRequests",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "DonationRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_DonationRequests_OrderLineId",
                table: "DonationRequests",
                column: "OrderLineId");

            migrationBuilder.AddForeignKey(
                name: "FK_DonationRequests_Centers_AssignedCenterId",
                table: "DonationRequests",
                column: "AssignedCenterId",
                principalTable: "Centers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DonationRequests_OrderLines_OrderLineId",
                table: "DonationRequests",
                column: "OrderLineId",
                principalTable: "OrderLines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DonationRequests_Centers_AssignedCenterId",
                table: "DonationRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_DonationRequests_OrderLines_OrderLineId",
                table: "DonationRequests");

            migrationBuilder.DropIndex(
                name: "IX_DonationRequests_OrderLineId",
                table: "DonationRequests");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "DonationRequests");

            migrationBuilder.AlterColumn<double>(
                name: "Weight",
                table: "Stocks",
                type: "double",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ShipmentDate",
                table: "DonationRequests",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "OrderLineId",
                table: "DonationRequests",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "AssignedCenterId",
                table: "DonationRequests",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLines_DonationRequestId",
                table: "OrderLines",
                column: "DonationRequestId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DonationRequests_Centers_AssignedCenterId",
                table: "DonationRequests",
                column: "AssignedCenterId",
                principalTable: "Centers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLines_DonationRequests_DonationRequestId",
                table: "OrderLines",
                column: "DonationRequestId",
                principalTable: "DonationRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
