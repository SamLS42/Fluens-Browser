using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fluens.Data.Migrations;

/// <inheritdoc />
public partial class BrowserWindowRequiredForTabs : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Tabs_BrowserWindows_BrowserWindowId",
            table: "Tabs");

        migrationBuilder.AlterColumn<int>(
            name: "BrowserWindowId",
            table: "Tabs",
            type: "INTEGER",
            nullable: false,
            defaultValue: 1,
            oldClrType: typeof(int),
            oldType: "INTEGER",
            oldNullable: true);

        migrationBuilder.AddForeignKey(
            name: "FK_Tabs_BrowserWindows_BrowserWindowId",
            table: "Tabs",
            column: "BrowserWindowId",
            principalTable: "BrowserWindows",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Tabs_BrowserWindows_BrowserWindowId",
            table: "Tabs");

        migrationBuilder.AlterColumn<int>(
            name: "BrowserWindowId",
            table: "Tabs",
            type: "INTEGER",
            nullable: true,
            oldClrType: typeof(int),
            oldType: "INTEGER");

        migrationBuilder.AddForeignKey(
            name: "FK_Tabs_BrowserWindows_BrowserWindowId",
            table: "Tabs",
            column: "BrowserWindowId",
            principalTable: "BrowserWindows",
            principalColumn: "Id");
    }
}
