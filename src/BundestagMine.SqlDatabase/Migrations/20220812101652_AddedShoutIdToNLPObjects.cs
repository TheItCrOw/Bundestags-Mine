using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BundestagMine.SqlDatabase.Migrations
{
    public partial class AddedShoutIdToNLPObjects : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ShoutId",
                table: "Token",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ShoutId",
                table: "Sentiment",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ShoutId",
                table: "NamedEntity",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShoutId",
                table: "Token");

            migrationBuilder.DropColumn(
                name: "ShoutId",
                table: "Sentiment");

            migrationBuilder.DropColumn(
                name: "ShoutId",
                table: "NamedEntity");
        }
    }
}
