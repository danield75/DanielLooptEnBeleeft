using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DanielLooptEnBeleeftApi.Migrations
{
    /// <inheritdoc />
    public partial class AddStravaSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "StravaActivityId",
                table: "RunningRaces",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StravaTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccessToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpiresAt = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StravaTokens", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StravaTokens");

            migrationBuilder.DropColumn(
                name: "StravaActivityId",
                table: "RunningRaces");
        }
    }
}
