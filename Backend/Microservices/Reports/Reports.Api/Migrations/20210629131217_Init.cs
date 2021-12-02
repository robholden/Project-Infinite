using Microsoft.EntityFrameworkCore.Migrations;

namespace Reports.Api.Migrations;

public partial class Init : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Actions",
            columns: table => new
            {
                ActionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                ActionTaken = table.Column<int>(type: "int", nullable: false),
                Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                Date = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Actions", x => x.ActionId);
            });

        migrationBuilder.CreateTable(
            name: "PictureReports",
            columns: table => new
            {
                ReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PictureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PictureName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                PicturePath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                ActionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Date = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PictureReports", x => x.ReportId);
                table.ForeignKey(
                    name: "FK_PictureReports_Actions_ActionId",
                    column: x => x.ActionId,
                    principalTable: "Actions",
                    principalColumn: "ActionId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "UserReports",
            columns: table => new
            {
                ReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                ActionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Date = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserReports", x => x.ReportId);
                table.ForeignKey(
                    name: "FK_UserReports_Actions_ActionId",
                    column: x => x.ActionId,
                    principalTable: "Actions",
                    principalColumn: "ActionId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "PictureReportInstances",
            columns: table => new
            {
                InstanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Reason = table.Column<int>(type: "int", nullable: false),
                ReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Date = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PictureReportInstances", x => x.InstanceId);
                table.ForeignKey(
                    name: "FK_PictureReportInstances_PictureReports_ReportId",
                    column: x => x.ReportId,
                    principalTable: "PictureReports",
                    principalColumn: "ReportId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "UserReportInstances",
            columns: table => new
            {
                InstanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Reason = table.Column<int>(type: "int", nullable: false),
                ReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Date = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserReportInstances", x => x.InstanceId);
                table.ForeignKey(
                    name: "FK_UserReportInstances_UserReports_ReportId",
                    column: x => x.ReportId,
                    principalTable: "UserReports",
                    principalColumn: "ReportId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PictureReportInstances_ReportId",
            table: "PictureReportInstances",
            column: "ReportId");

        migrationBuilder.CreateIndex(
            name: "IX_PictureReports_ActionId",
            table: "PictureReports",
            column: "ActionId");

        migrationBuilder.CreateIndex(
            name: "IX_UserReportInstances_ReportId",
            table: "UserReportInstances",
            column: "ReportId");

        migrationBuilder.CreateIndex(
            name: "IX_UserReports_ActionId",
            table: "UserReports",
            column: "ActionId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "PictureReportInstances");

        migrationBuilder.DropTable(
            name: "UserReportInstances");

        migrationBuilder.DropTable(
            name: "PictureReports");

        migrationBuilder.DropTable(
            name: "UserReports");

        migrationBuilder.DropTable(
            name: "Actions");
    }
}
