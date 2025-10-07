using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HiLoGame.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomPlayerUsername : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerUsername",
                table: "Rooms",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PlayerUsername",
                table: "RoomPlayers",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerUsername",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "PlayerUsername",
                table: "RoomPlayers");
        }
    }
}
