using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BundestagMine.SqlDatabase.Migrations
{
    public partial class AddedDailyPaperSubscription : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyPaperSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    InitialSubscriptionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSendTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSentDailyPaperId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyPaperSubscriptions", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyPaperSubscriptions");
        }
    }
}
