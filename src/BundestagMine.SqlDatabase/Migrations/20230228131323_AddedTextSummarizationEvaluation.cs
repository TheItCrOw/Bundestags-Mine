using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BundestagMine.SqlDatabase.Migrations
{
    public partial class AddedTextSummarizationEvaluation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TextSummarizationEvaluationScores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SpeechId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TextSummarizationMethod = table.Column<int>(type: "int", nullable: false),
                    NamedEntityDistance = table.Column<double>(type: "float", nullable: false),
                    LevenstheinSimilaritiesOfSentences = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AverageWordsPerSentence = table.Column<int>(type: "int", nullable: false),
                    SummaryCompressionRate = table.Column<double>(type: "float", nullable: false),
                    ScoreExplanation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SummaryScore = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TextSummarizationEvaluationScores", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TextSummarizationEvaluationScores");
        }
    }
}
