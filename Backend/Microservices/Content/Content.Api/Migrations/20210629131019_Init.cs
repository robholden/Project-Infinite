using Microsoft.EntityFrameworkCore.Migrations;

namespace Content.Api.Migrations;

public partial class Init : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Collections",
            columns: table => new
            {
                CollectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Collections", x => x.CollectionId);
            });

        migrationBuilder.CreateTable(
            name: "Countries",
            columns: table => new
            {
                CountryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                Lat = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                Lng = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Countries", x => x.CountryId);
            });

        migrationBuilder.CreateTable(
            name: "PictureLocationRequests",
            columns: table => new
            {
                RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                OwnedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                Lat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                Lng = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PictureLocationRequests", x => x.RequestId);
            });

        migrationBuilder.CreateTable(
            name: "Tags",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Value = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                Weight = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Tags", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "UserSettings",
            columns: table => new
            {
                SettingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                MaxPictureSize = table.Column<int>(type: "int", nullable: false),
                MinPictureResolutionX = table.Column<int>(type: "int", nullable: false),
                MinPictureResolutionY = table.Column<int>(type: "int", nullable: false),
                UploadLimit = table.Column<int>(type: "int", nullable: false),
                UploadEnabled = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserSettings", x => x.SettingId);
            });

        migrationBuilder.CreateTable(
            name: "Locations",
            columns: table => new
            {
                LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                CountryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Lat = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                Lng = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Locations", x => x.LocationId);
                table.ForeignKey(
                    name: "FK_Locations_Countries_CountryId",
                    column: x => x.CountryId,
                    principalTable: "Countries",
                    principalColumn: "CountryId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Boundries",
            columns: table => new
            {
                BoundryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                MinLat = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                MaxLat = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                MinLng = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                MaxLng = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Boundries", x => x.BoundryId);
                table.ForeignKey(
                    name: "FK_Boundries_Locations_LocationId",
                    column: x => x.LocationId,
                    principalTable: "Locations",
                    principalColumn: "LocationId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Pictures",
            columns: table => new
            {
                PictureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                IpAddress = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                Colours = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                DateTaken = table.Column<DateTime>(type: "datetime2", nullable: false),
                Format = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Hash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Width = table.Column<int>(type: "int", nullable: false),
                Height = table.Column<int>(type: "int", nullable: false),
                ConcealCoords = table.Column<bool>(type: "bit", nullable: false),
                LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Ext = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                SeedKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Status = table.Column<int>(type: "int", nullable: false),
                Featured = table.Column<bool>(type: "bit", nullable: false),
                CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                Lat = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                Lng = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Pictures", x => x.PictureId);
                table.ForeignKey(
                    name: "FK_Pictures_Locations_LocationId",
                    column: x => x.LocationId,
                    principalTable: "Locations",
                    principalColumn: "LocationId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "CollectionPicture",
            columns: table => new
            {
                CollectionsCollectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PicturesPictureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CollectionPicture", x => new { x.CollectionsCollectionId, x.PicturesPictureId });
                table.ForeignKey(
                    name: "FK_CollectionPicture_Collections_CollectionsCollectionId",
                    column: x => x.CollectionsCollectionId,
                    principalTable: "Collections",
                    principalColumn: "CollectionId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_CollectionPicture_Pictures_PicturesPictureId",
                    column: x => x.PicturesPictureId,
                    principalTable: "Pictures",
                    principalColumn: "PictureId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "PictureLikes",
            columns: table => new
            {
                PictureLikeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PictureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PictureLikes", x => new { x.PictureLikeId, x.UserId, x.PictureId });
                table.ForeignKey(
                    name: "FK_PictureLikes_Pictures_PictureId",
                    column: x => x.PictureId,
                    principalTable: "Pictures",
                    principalColumn: "PictureId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "PictureModerations",
            columns: table => new
            {
                ModerationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PictureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                LockedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                LockedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PictureModerations", x => new { x.ModerationId, x.PictureId });
                table.ForeignKey(
                    name: "FK_PictureModerations_Pictures_PictureId",
                    column: x => x.PictureId,
                    principalTable: "Pictures",
                    principalColumn: "PictureId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "PictureTag",
            columns: table => new
            {
                PicturesPictureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TagsId = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PictureTag", x => new { x.PicturesPictureId, x.TagsId });
                table.ForeignKey(
                    name: "FK_PictureTag_Pictures_PicturesPictureId",
                    column: x => x.PicturesPictureId,
                    principalTable: "Pictures",
                    principalColumn: "PictureId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_PictureTag_Tags_TagsId",
                    column: x => x.TagsId,
                    principalTable: "Tags",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Boundries_LocationId",
            table: "Boundries",
            column: "LocationId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_CollectionPicture_PicturesPictureId",
            table: "CollectionPicture",
            column: "PicturesPictureId");

        migrationBuilder.CreateIndex(
            name: "IX_Locations_CountryId",
            table: "Locations",
            column: "CountryId");

        migrationBuilder.CreateIndex(
            name: "IX_PictureLikes_PictureId",
            table: "PictureLikes",
            column: "PictureId");

        migrationBuilder.CreateIndex(
            name: "IX_PictureModerations_PictureId",
            table: "PictureModerations",
            column: "PictureId");

        migrationBuilder.CreateIndex(
            name: "IX_Pictures_LocationId",
            table: "Pictures",
            column: "LocationId");

        migrationBuilder.CreateIndex(
            name: "IX_PictureTag_TagsId",
            table: "PictureTag",
            column: "TagsId");

        migrationBuilder.CreateIndex(
            name: "IX_Tags_Value",
            table: "Tags",
            column: "Value",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Boundries");

        migrationBuilder.DropTable(
            name: "CollectionPicture");

        migrationBuilder.DropTable(
            name: "PictureLikes");

        migrationBuilder.DropTable(
            name: "PictureLocationRequests");

        migrationBuilder.DropTable(
            name: "PictureModerations");

        migrationBuilder.DropTable(
            name: "PictureTag");

        migrationBuilder.DropTable(
            name: "UserSettings");

        migrationBuilder.DropTable(
            name: "Collections");

        migrationBuilder.DropTable(
            name: "Pictures");

        migrationBuilder.DropTable(
            name: "Tags");

        migrationBuilder.DropTable(
            name: "Locations");

        migrationBuilder.DropTable(
            name: "Countries");
    }
}
