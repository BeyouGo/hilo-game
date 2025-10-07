using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HiLoGame.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUpdateRoomScoreAndMaxPlayerCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxRounds",
                table: "Rooms");

            migrationBuilder.RenameColumn(
                name: "Score",
                table: "RoomPlayers",
                newName: "Attempts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Attempts",
                table: "RoomPlayers",
                newName: "Score");

            migrationBuilder.AddColumn<int>(
                name: "MaxRounds",
                table: "Rooms",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
