using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoCaritas.Migrations
{
    /// <inheritdoc />
    public partial class NotificationsRecipientUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "RecipientCenterId",
                table: "Notifications",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "RecipientUserId",
                table: "Notifications",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecipientUserId",
                table: "Notifications");

            migrationBuilder.AlterColumn<int>(
                name: "RecipientCenterId",
                table: "Notifications",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
