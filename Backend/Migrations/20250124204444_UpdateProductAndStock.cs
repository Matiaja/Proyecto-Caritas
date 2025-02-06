using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoCaritas.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProductAndStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Stocks");

            migrationBuilder.RenameColumn(
                name: "EntryDate",
                table: "Stocks",
                newName: "Date");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Stocks",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Products",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Stocks");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Stocks",
                newName: "EntryDate");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Stocks",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
