using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CineVault.API.Migrations
{
    /// <inheritdoc />
    public partial class MiscSchemaChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "Users",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Review_Rating",
                table: "Reviews",
                sql: "Rating BETWEEN 1 AND 10");

            migrationBuilder.CreateIndex(
                name: "IX_Movies_ReleaseDate",
                table: "Movies",
                column: "ReleaseDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Review_Rating",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Movies_ReleaseDate",
                table: "Movies");

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);
        }
    }
}
