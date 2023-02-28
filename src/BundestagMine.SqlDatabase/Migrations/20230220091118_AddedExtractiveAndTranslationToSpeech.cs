using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BundestagMine.SqlDatabase.Migrations
{
    public partial class AddedExtractiveAndTranslationToSpeech : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EnglishTranslationOfSpeech",
                table: "Speeches",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExtractiveSummary",
                table: "Speeches",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnglishTranslationOfSpeech",
                table: "Speeches");

            migrationBuilder.DropColumn(
                name: "ExtractiveSummary",
                table: "Speeches");
        }
    }
}
