using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoCaritas.Migrations
{
    /// <inheritdoc />
    public partial class UserControllerUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StorageCenterId",
                table: "AspNetUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StorageCenterId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);
        }
    }
}
