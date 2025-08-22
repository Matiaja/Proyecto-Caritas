using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoCaritas.Migrations
{
    /// <inheritdoc />
    public partial class AddOriginalCenterPurchase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BuyerName",
                table: "Purchases",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "OriginalCenterId",
                table: "Purchases",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_OriginalCenterId",
                table: "Purchases",
                column: "OriginalCenterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_Centers_OriginalCenterId",
                table: "Purchases",
                column: "OriginalCenterId",
                principalTable: "Centers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_Centers_OriginalCenterId",
                table: "Purchases");

            migrationBuilder.DropIndex(
                name: "IX_Purchases_OriginalCenterId",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "BuyerName",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "OriginalCenterId",
                table: "Purchases");
        }
    }
}
