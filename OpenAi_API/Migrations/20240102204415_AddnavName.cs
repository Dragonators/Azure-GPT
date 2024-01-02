using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenAi_API.Migrations
{
    /// <inheritdoc />
    public partial class AddnavName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "navName",
                table: "Navlinks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "navName",
                table: "Navlinks");
        }
    }
}
