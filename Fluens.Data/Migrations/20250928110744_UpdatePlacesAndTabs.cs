using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fluens.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePlacesAndTabs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentTitle",
                table: "Tabs");

            migrationBuilder.DropColumn(
                name: "FaviconUrl",
                table: "Tabs");

            migrationBuilder.DropColumn(
                name: "Url",
                table: "Tabs");

            migrationBuilder.AddColumn<int>(
                name: "PlaceId",
                table: "Tabs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tabs_PlaceId",
                table: "Tabs",
                column: "PlaceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tabs_Places_PlaceId",
                table: "Tabs",
                column: "PlaceId",
                principalTable: "Places",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tabs_Places_PlaceId",
                table: "Tabs");

            migrationBuilder.DropIndex(
                name: "IX_Tabs_PlaceId",
                table: "Tabs");

            migrationBuilder.DropColumn(
                name: "PlaceId",
                table: "Tabs");

            migrationBuilder.AddColumn<string>(
                name: "DocumentTitle",
                table: "Tabs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FaviconUrl",
                table: "Tabs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "Tabs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
