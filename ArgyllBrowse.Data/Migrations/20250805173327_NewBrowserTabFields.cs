using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArgyllBrowse.Data.Migrations
{
    /// <inheritdoc />
    public partial class NewBrowserTabFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DocumentTitle",
                table: "OpenTabs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FaviconUrl",
                table: "OpenTabs",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentTitle",
                table: "OpenTabs");

            migrationBuilder.DropColumn(
                name: "FaviconUrl",
                table: "OpenTabs");
        }
    }
}
