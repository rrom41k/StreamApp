using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreamAppApi.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRatingMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Rating",
                table: "UserMovies",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rating",
                table: "UserMovies");
        }
    }
}
