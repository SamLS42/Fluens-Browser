using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fluens.Data.Migrations;

/// <inheritdoc />
public partial class AddHistoryLastVisitedOnIndex : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_History_LastVisitedOn",
            table: "History",
            column: "LastVisitedOn");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_History_LastVisitedOn",
            table: "History");
    }
}
