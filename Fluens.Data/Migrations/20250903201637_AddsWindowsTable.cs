using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fluens.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddsWindowsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsTabSelected",
                table: "Tabs",
                newName: "IsSelected");

            migrationBuilder.AddColumn<int>(
                name: "BrowserWindowId",
                table: "Tabs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BrowserWindows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    X = table.Column<int>(type: "INTEGER", nullable: false),
                    Y = table.Column<int>(type: "INTEGER", nullable: false),
                    Width = table.Column<int>(type: "INTEGER", nullable: false),
                    Height = table.Column<int>(type: "INTEGER", nullable: false),
                    IsMaximized = table.Column<bool>(type: "INTEGER", nullable: false),
                    ClosedOn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BrowserWindows", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tabs_BrowserWindowId",
                table: "Tabs",
                column: "BrowserWindowId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tabs_BrowserWindows_BrowserWindowId",
                table: "Tabs",
                column: "BrowserWindowId",
                principalTable: "BrowserWindows",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tabs_BrowserWindows_BrowserWindowId",
                table: "Tabs");

            migrationBuilder.DropTable(
                name: "BrowserWindows");

            migrationBuilder.DropIndex(
                name: "IX_Tabs_BrowserWindowId",
                table: "Tabs");

            migrationBuilder.DropColumn(
                name: "BrowserWindowId",
                table: "Tabs");

            migrationBuilder.RenameColumn(
                name: "IsSelected",
                table: "Tabs",
                newName: "IsTabSelected");
        }
    }
}
