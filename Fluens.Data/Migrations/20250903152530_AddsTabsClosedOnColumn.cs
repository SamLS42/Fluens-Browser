using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fluens.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddsTabsClosedOnColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_OpenTabs",
                table: "OpenTabs");

            migrationBuilder.RenameTable(
                name: "OpenTabs",
                newName: "Tabs");

            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedOn",
                table: "Tabs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tabs",
                table: "Tabs",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Tabs",
                table: "Tabs");

            migrationBuilder.DropColumn(
                name: "ClosedOn",
                table: "Tabs");

            migrationBuilder.RenameTable(
                name: "Tabs",
                newName: "OpenTabs");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OpenTabs",
                table: "OpenTabs",
                column: "Id");
        }
    }
}
