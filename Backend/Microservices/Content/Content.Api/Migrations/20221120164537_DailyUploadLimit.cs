using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Api.Migrations
{
    /// <inheritdoc />
    public partial class DailyUploadLimit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UploadLimit",
                table: "UserSettings",
                newName: "DraftLimit");

            migrationBuilder.AddColumn<int>(
                name: "DailyUploadLimit",
                table: "UserSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailyUploadLimit",
                table: "UserSettings");

            migrationBuilder.RenameColumn(
                name: "DraftLimit",
                table: "UserSettings",
                newName: "UploadLimit");
        }
    }
}
