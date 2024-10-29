using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BundestagMine.SqlDatabase.Migrations
{
    public partial class AddedEnglishTranslationScore : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "EnglishTranslationScore",
                table: "Speeches",
                type: "float",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnglishTranslationScore",
                table: "Speeches");
        }
    }
}
