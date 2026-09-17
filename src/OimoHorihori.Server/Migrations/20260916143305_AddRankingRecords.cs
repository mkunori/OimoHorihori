using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OimoHorihori.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddRankingRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RankingRecords",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    TotalPotato = table.Column<double>(type: "REAL", nullable: false),
                    BestProductionPerSecond = table.Column<double>(type: "REAL", nullable: false),
                    ReplantCount = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RankingRecords", x => x.UserId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RankingRecords_BestProductionPerSecond",
                table: "RankingRecords",
                column: "BestProductionPerSecond");

            migrationBuilder.CreateIndex(
                name: "IX_RankingRecords_ReplantCount",
                table: "RankingRecords",
                column: "ReplantCount");

            migrationBuilder.CreateIndex(
                name: "IX_RankingRecords_TotalPotato",
                table: "RankingRecords",
                column: "TotalPotato");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RankingRecords");
        }
    }
}
