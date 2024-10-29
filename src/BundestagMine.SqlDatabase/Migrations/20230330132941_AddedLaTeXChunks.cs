using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BundestagMine.SqlDatabase.Migrations
{
    public partial class AddedLaTeXChunks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LaTeXChunks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChunkType = table.Column<int>(type: "int", nullable: false),
                    LaTeX = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LaTeXChunks", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LaTeXChunks");
        }
    }
}
