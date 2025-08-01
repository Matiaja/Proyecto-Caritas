using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoCaritas.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceptionDate",
                table: "DonationRequests");

            migrationBuilder.RenameColumn(
                name: "ShipmentDate",
                table: "DonationRequests",
                newName: "LastStatusChangeDate");

            migrationBuilder.RenameColumn(
                name: "AsignationDate",
                table: "DonationRequests",
                newName: "AssignmentDate");

            migrationBuilder.CreateTable(
                name: "DonationRequestStatusHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DonationRequestId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ChangeDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonationRequestStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DonationRequestStatusHistories_DonationRequests_DonationRequ~",
                        column: x => x.DonationRequestId,
                        principalTable: "DonationRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_DonationRequestStatusHistories_DonationRequestId",
                table: "DonationRequestStatusHistories",
                column: "DonationRequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DonationRequestStatusHistories");

            migrationBuilder.RenameColumn(
                name: "LastStatusChangeDate",
                table: "DonationRequests",
                newName: "ShipmentDate");

            migrationBuilder.RenameColumn(
                name: "AssignmentDate",
                table: "DonationRequests",
                newName: "AsignationDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "ReceptionDate",
                table: "DonationRequests",
                type: "datetime(6)",
                nullable: true);
        }
    }
}
