using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoCaritas.Migrations
{
    /// <inheritdoc />
    public partial class AddOriginalCenterPurchase2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_Centers_OriginalCenterId",
                table: "Purchases");

            migrationBuilder.AlterColumn<int>(
                name: "OriginalCenterId",
                table: "Purchases",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_Centers_OriginalCenterId",
                table: "Purchases",
                column: "OriginalCenterId",
                principalTable: "Centers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_Centers_OriginalCenterId",
                table: "Purchases");

            migrationBuilder.AlterColumn<int>(
                name: "OriginalCenterId",
                table: "Purchases",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_Centers_OriginalCenterId",
                table: "Purchases",
                column: "OriginalCenterId",
                principalTable: "Centers",
                principalColumn: "Id");
        }
    }
}
