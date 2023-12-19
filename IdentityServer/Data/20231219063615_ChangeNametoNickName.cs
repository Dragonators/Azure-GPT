using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityServer.Data
{
    /// <inheritdoc />
    public partial class ChangeNametoNickName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "AspNetUsers",
                newName: "NickName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NickName",
                table: "AspNetUsers",
                newName: "Name");
        }
    }
}
