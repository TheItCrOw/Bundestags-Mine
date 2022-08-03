using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BundestagMine.SqlDatabase.Migrations
{
    public partial class AddedLemmaValueToNE : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LemmaValue",
                table: "NamedEntity",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LemmaValue",
                table: "NamedEntity");
        }
    }
}
