using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DanielLooptEnBeleeftApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RunningRaces",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Datum = table.Column<DateOnly>(type: "date", nullable: false),
                    Distance = table.Column<int>(type: "int", nullable: false),
                    DistanceUnit = table.Column<int>(type: "int", nullable: false),
                    RaceName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RacePlace = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RaceStartTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    LinkToRaceWebsite = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FinishTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    OverallPlace = table.Column<int>(type: "int", nullable: true),
                    NumberParticipantsOverall = table.Column<int>(type: "int", nullable: true),
                    AgeCategoryPlace = table.Column<int>(type: "int", nullable: true),
                    NumberParticipantsAgeCategory = table.Column<int>(type: "int", nullable: true),
                    LinkToRaceResult = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    LinkToRaceReport = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RunningRaces", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RaceReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RunningRaceId = table.Column<int>(type: "int", nullable: false),
                    ReportText = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaceReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RaceReports_RunningRaces_RunningRaceId",
                        column: x => x.RunningRaceId,
                        principalTable: "RunningRaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RaceReports_RunningRaceId",
                table: "RaceReports",
                column: "RunningRaceId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RaceReports");

            migrationBuilder.DropTable(
                name: "RunningRaces");
        }
    }
}
