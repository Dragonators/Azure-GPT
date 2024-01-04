using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenAi_API.Migrations
{
    /// <inheritdoc />
    public partial class changeCreateATtolatestAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "createAt",
                table: "Navlinks",
                newName: "latestAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "latestAt",
                table: "Navlinks",
                newName: "createAt");
        }
    }
}
