using Microsoft.EntityFrameworkCore.Migrations;

namespace Comms.Api.Migrations
{
    public partial class NotificationEmail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PictureUnapproved",
                table: "UserSettings",
                newName: "PictureUnapprovedEmail");

            migrationBuilder.RenameColumn(
                name: "PictureLiked",
                table: "UserSettings",
                newName: "PictureLikedEmail");

            migrationBuilder.RenameColumn(
                name: "PictureApproved",
                table: "UserSettings",
                newName: "PictureApprovedEmail");

            migrationBuilder.RenameColumn(
                name: "Marketing",
                table: "UserSettings",
                newName: "MarketingEmail");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PictureUnapprovedEmail",
                table: "UserSettings",
                newName: "PictureUnapproved");

            migrationBuilder.RenameColumn(
                name: "PictureLikedEmail",
                table: "UserSettings",
                newName: "PictureLiked");

            migrationBuilder.RenameColumn(
                name: "PictureApprovedEmail",
                table: "UserSettings",
                newName: "PictureApproved");

            migrationBuilder.RenameColumn(
                name: "MarketingEmail",
                table: "UserSettings",
                newName: "Marketing");
        }
    }
}
