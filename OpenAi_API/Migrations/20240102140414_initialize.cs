using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenAi_API.Migrations
{
    /// <inheritdoc />
    public partial class initialize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Navlinks",
                columns: table => new
                {
                    navId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    createAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    userId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Navlinks", x => x.navId);
                });

            migrationBuilder.CreateTable(
                name: "HistoryMessages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    navId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    creatAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoryMessages_Navlinks_navId",
                        column: x => x.navId,
                        principalTable: "Navlinks",
                        principalColumn: "navId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistoryMessages_navId",
                table: "HistoryMessages",
                column: "navId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistoryMessages");

            migrationBuilder.DropTable(
                name: "Navlinks");
        }
    }
}
