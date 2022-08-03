using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BundestagMine.SqlDatabase.Migrations
{
    public partial class AddedNLPSpeechj : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Speeches",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "CategoryCoveredTagged",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Begin = table.Column<int>(type: "int", nullable: false),
                    End = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Score = table.Column<double>(type: "float", nullable: false),
                    NLPSpeechId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryCoveredTagged", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryCoveredTagged_Speeches_NLPSpeechId",
                        column: x => x.NLPSpeechId,
                        principalTable: "Speeches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NamedEntity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Begin = table.Column<int>(type: "int", nullable: false),
                    End = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NLPSpeechId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NamedEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NamedEntity_Speeches_NLPSpeechId",
                        column: x => x.NLPSpeechId,
                        principalTable: "Speeches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Sentiment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Begin = table.Column<int>(type: "int", nullable: false),
                    End = table.Column<int>(type: "int", nullable: false),
                    SentimentSingleScore = table.Column<double>(type: "float", nullable: false),
                    NLPSpeechId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sentiment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sentiment_Speeches_NLPSpeechId",
                        column: x => x.NLPSpeechId,
                        principalTable: "Speeches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Token",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Begin = table.Column<int>(type: "int", nullable: false),
                    End = table.Column<int>(type: "int", nullable: false),
                    LemmaValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Stem = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    posValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Morph = table.Column<bool>(type: "bit", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Number = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Case = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Degree = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Aspect = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VerbForm = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tense = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mood = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Voice = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Definiteness = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Person = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Animacy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Negative = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Possessive = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PronType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reflex = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Transitivity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NLPSpeechId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Token", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Token_Speeches_NLPSpeechId",
                        column: x => x.NLPSpeechId,
                        principalTable: "Speeches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CategoryCoveredTagged_NLPSpeechId",
                table: "CategoryCoveredTagged",
                column: "NLPSpeechId");

            migrationBuilder.CreateIndex(
                name: "IX_NamedEntity_NLPSpeechId",
                table: "NamedEntity",
                column: "NLPSpeechId");

            migrationBuilder.CreateIndex(
                name: "IX_Sentiment_NLPSpeechId",
                table: "Sentiment",
                column: "NLPSpeechId");

            migrationBuilder.CreateIndex(
                name: "IX_Token_NLPSpeechId",
                table: "Token",
                column: "NLPSpeechId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategoryCoveredTagged");

            migrationBuilder.DropTable(
                name: "NamedEntity");

            migrationBuilder.DropTable(
                name: "Sentiment");

            migrationBuilder.DropTable(
                name: "Token");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Speeches");
        }
    }
}
