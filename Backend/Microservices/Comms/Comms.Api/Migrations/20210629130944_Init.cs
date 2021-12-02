using Microsoft.EntityFrameworkCore.Migrations;

namespace Comms.Api.Migrations;

public partial class Init : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Notifications",
            columns: table => new
            {
                NotificationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserLevel = table.Column<int>(type: "int", nullable: true),
                Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ContentKey = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                ContentImage = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                ContentMessage = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                ViewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                Type = table.Column<int>(type: "int", nullable: false),
                Hidden = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Notifications", x => x.NotificationId);
            });

        migrationBuilder.CreateTable(
            name: "Sms",
            columns: table => new
            {
                SmsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Message = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Mobile = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                DateSent = table.Column<DateTime>(type: "datetime2", nullable: true),
                Sent = table.Column<bool>(type: "bit", nullable: false),
                Date = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Sms", x => x.SmsId);
            });

        migrationBuilder.CreateTable(
            name: "UserSettings",
            columns: table => new
            {
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                MarketingOptOutKey = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Marketing = table.Column<bool>(type: "bit", nullable: false),
                PictureApproved = table.Column<bool>(type: "bit", nullable: false),
                PictureUnapproved = table.Column<bool>(type: "bit", nullable: false),
                PictureLiked = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserSettings", x => x.UserId);
            });

        migrationBuilder.CreateTable(
            name: "EmailQueue",
            columns: table => new
            {
                EmailQueueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                EmailAddress = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                IdentityHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                Subject = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                Type = table.Column<int>(type: "int", nullable: false),
                OwnedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Completed = table.Column<bool>(type: "bit", nullable: false),
                CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EmailQueue", x => x.EmailQueueId);
                table.ForeignKey(
                    name: "FK_EmailQueue_UserSettings_UserId",
                    column: x => x.UserId,
                    principalTable: "UserSettings",
                    principalColumn: "UserId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "NotificationEntries",
            columns: table => new
            {
                EntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                NotificationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                Sent = table.Column<bool>(type: "bit", nullable: false),
                Deleted = table.Column<bool>(type: "bit", nullable: false),
                UserSettingUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_NotificationEntries", x => x.EntryId);
                table.ForeignKey(
                    name: "FK_NotificationEntries_Notifications_NotificationId",
                    column: x => x.NotificationId,
                    principalTable: "Notifications",
                    principalColumn: "NotificationId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_NotificationEntries_UserSettings_UserSettingUserId",
                    column: x => x.UserSettingUserId,
                    principalTable: "UserSettings",
                    principalColumn: "UserId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Emails",
            columns: table => new
            {
                EmailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Completed = table.Column<bool>(type: "bit", nullable: false),
                Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                DateSent = table.Column<DateTime>(type: "datetime2", nullable: true),
                EmailQueueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Errors = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                FromEmailAddress = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                Attempts = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Emails", x => x.EmailId);
                table.ForeignKey(
                    name: "FK_Emails_EmailQueue_EmailQueueId",
                    column: x => x.EmailQueueId,
                    principalTable: "EmailQueue",
                    principalColumn: "EmailQueueId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_EmailQueue_UserId",
            table: "EmailQueue",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_Emails_EmailQueueId",
            table: "Emails",
            column: "EmailQueueId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_NotificationEntries_NotificationId",
            table: "NotificationEntries",
            column: "NotificationId");

        migrationBuilder.CreateIndex(
            name: "IX_NotificationEntries_UserSettingUserId",
            table: "NotificationEntries",
            column: "UserSettingUserId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Emails");

        migrationBuilder.DropTable(
            name: "NotificationEntries");

        migrationBuilder.DropTable(
            name: "Sms");

        migrationBuilder.DropTable(
            name: "EmailQueue");

        migrationBuilder.DropTable(
            name: "Notifications");

        migrationBuilder.DropTable(
            name: "UserSettings");
    }
}
