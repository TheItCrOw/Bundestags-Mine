using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BundestagMine.SqlDatabase.Migrations
{
    public partial class AddedSpeechesToDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Speeches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpeakerId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProtocolNumber = table.Column<int>(type: "int", nullable: false),
                    LegislaturePeriod = table.Column<int>(type: "int", nullable: false),
                    AgendaItemNumber = table.Column<int>(type: "int", nullable: false),
                    MongoId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Speeches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SpeechSegment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpeechId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpeechSegment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpeechSegment_Speeches_SpeechId",
                        column: x => x.SpeechId,
                        principalTable: "Speeches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Shouts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Fraction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Party = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpeakerId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpeechSegmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shouts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shouts_SpeechSegment_SpeechSegmentId",
                        column: x => x.SpeechSegmentId,
                        principalTable: "SpeechSegment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Shouts_SpeechSegmentId",
                table: "Shouts",
                column: "SpeechSegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_SpeechSegment_SpeechId",
                table: "SpeechSegment",
                column: "SpeechId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Shouts");

            migrationBuilder.DropTable(
                name: "SpeechSegment");

            migrationBuilder.DropTable(
                name: "Speeches");
        }
    }
}
