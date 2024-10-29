using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BundestagMine.SqlDatabase.Migrations
{
    public partial class AddedDailyPapers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyPapers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LegislaturePeriod = table.Column<int>(type: "int", nullable: false),
                    ProtocolNumber = table.Column<int>(type: "int", nullable: false),
                    ProtocolDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    JsonDataString = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyPapers", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyPapers");
        }
    }
}
