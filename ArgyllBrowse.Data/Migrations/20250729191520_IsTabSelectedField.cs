using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArgyllBrowse.Data.Migrations
{
    /// <inheritdoc />
    public partial class IsTabSelectedField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTabSelected",
                table: "OpenTabs",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTabSelected",
                table: "OpenTabs");
        }
    }
}
