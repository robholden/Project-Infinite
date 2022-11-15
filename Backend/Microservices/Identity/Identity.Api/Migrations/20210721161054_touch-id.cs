using Microsoft.EntityFrameworkCore.Migrations;

namespace Identity.Api.Migrations;

public partial class TouchId : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "TouchIdEnabled",
            table: "AuthTokens",
            type: "bit",
            nullable: false,
            defaultValue: false);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "TouchIdEnabled",
            table: "AuthTokens");
    }
}
