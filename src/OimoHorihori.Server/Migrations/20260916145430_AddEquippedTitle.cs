using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OimoHorihori.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddEquippedTitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EquippedTitleId",
                table: "UserAccounts",
                type: "TEXT",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EquippedTitleId",
                table: "UserAccounts");
        }
    }
}
