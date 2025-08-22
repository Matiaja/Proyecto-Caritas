using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoCaritas.Migrations
{
    /// <inheritdoc />
    public partial class AddOriginalCenterPurchase3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_Centers_OriginalCenterId",
                table: "Purchases");

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_Centers_OriginalCenterId",
                table: "Purchases",
                column: "OriginalCenterId",
                principalTable: "Centers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_Centers_OriginalCenterId",
                table: "Purchases");

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_Centers_OriginalCenterId",
                table: "Purchases",
                column: "OriginalCenterId",
                principalTable: "Centers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
