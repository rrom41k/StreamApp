using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreamAppApi.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddReshreshTokenToUserMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "updatedAt",
                table: "Users",
                newName: "tokenUpdated");

            migrationBuilder.RenameColumn(
                name: "createdAt",
                table: "Users",
                newName: "tokenCreated");

            migrationBuilder.AddColumn<string>(
                name: "refreshToken",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "refreshToken",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "tokenUpdated",
                table: "Users",
                newName: "updatedAt");

            migrationBuilder.RenameColumn(
                name: "tokenCreated",
                table: "Users",
                newName: "createdAt");
        }
    }
}
