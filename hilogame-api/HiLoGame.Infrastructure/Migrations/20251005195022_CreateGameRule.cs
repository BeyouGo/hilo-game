using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HiLoGame.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateGameRule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxPlayers",
                table: "Rooms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxRounds",
                table: "Rooms",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxPlayers",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "MaxRounds",
                table: "Rooms");
        }
    }
}
