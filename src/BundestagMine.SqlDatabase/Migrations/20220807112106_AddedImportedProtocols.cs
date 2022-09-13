using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BundestagMine.SqlDatabase.Migrations
{
    public partial class AddedImportedProtocols : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImportedProtocols",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImportedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProtocolJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportedProtocols", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImportedProtocols");
        }
    }
}
