using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BundestagMine.SqlDatabase.Migrations
{
    public partial class AddedNetworkDataToDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Deputies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HistorySince = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeathDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaritalStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Religion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Profession = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Party = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Fraction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpeakerId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MongoId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deputies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NetworkDatas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MongoId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NetworkDatas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CommentNetworkLink",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Target = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sentiment = table.Column<double>(type: "float", nullable: false),
                    Value = table.Column<int>(type: "int", nullable: false),
                    NetworkDataId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentNetworkLink", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommentNetworkLink_NetworkDatas_NetworkDataId",
                        column: x => x.NetworkDataId,
                        principalTable: "NetworkDatas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CommentNetworkNode",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Party = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NetworkDataId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentNetworkNode", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommentNetworkNode_NetworkDatas_NetworkDataId",
                        column: x => x.NetworkDataId,
                        principalTable: "NetworkDatas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommentNetworkLink_NetworkDataId",
                table: "CommentNetworkLink",
                column: "NetworkDataId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentNetworkNode_NetworkDataId",
                table: "CommentNetworkNode",
                column: "NetworkDataId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommentNetworkLink");

            migrationBuilder.DropTable(
                name: "CommentNetworkNode");

            migrationBuilder.DropTable(
                name: "Deputies");

            migrationBuilder.DropTable(
                name: "NetworkDatas");
        }
    }
}
