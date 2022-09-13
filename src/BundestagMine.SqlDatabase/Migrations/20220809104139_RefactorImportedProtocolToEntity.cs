using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BundestagMine.SqlDatabase.Migrations
{
    public partial class RefactorImportedProtocolToEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImportedProtocols");

            migrationBuilder.CreateTable(
                name: "ImportedEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImportedDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModelJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    ProtocolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportedEntities", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImportedEntities");

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
    }
}
