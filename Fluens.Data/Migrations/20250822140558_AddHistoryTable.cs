using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fluens.Data.Migrations;

/// <inheritdoc />
public partial class AddHistoryTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "History",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Url = table.Column<string>(type: "TEXT", nullable: false),
                FaviconUrl = table.Column<string>(type: "TEXT", nullable: false),
                DocumentTitle = table.Column<string>(type: "TEXT", nullable: true),
                LastVisitedOn = table.Column<DateTime>(type: "TEXT", nullable: false),
                Host = table.Column<string>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_History", x => x.Id);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "History");
    }
}
