using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BundestagMine.SqlDatabase.Migrations.BundestagMineTokenDb
{
    public partial class AddedNewTokenDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Token",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NLPSpeechId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShoutId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    Transitivity = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Token", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Token");
        }
    }
}
