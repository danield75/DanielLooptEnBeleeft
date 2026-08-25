using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DanielLooptEnBeleeftApi.Migrations
{
    /// <inheritdoc />
    public partial class AddRaceCircuit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RaceCircuit",
                table: "RunningRaces",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RaceCircuit",
                table: "RunningRaces");
        }
    }
}
