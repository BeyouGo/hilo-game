using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HiLoGame.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomPlayerGuessInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FirstGuessAt",
                table: "RoomPlayers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastGuessAt",
                table: "RoomPlayers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SecretIsGreaterThan",
                table: "RoomPlayers",
                type: "int",
                nullable: false,
                defaultValue: -2147483648);

            migrationBuilder.AddColumn<int>(
                name: "SecretIsLessThan",
                table: "RoomPlayers",
                type: "int",
                nullable: false,
                defaultValue: 2147483647);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstGuessAt",
                table: "RoomPlayers");

            migrationBuilder.DropColumn(
                name: "LastGuessAt",
                table: "RoomPlayers");

            migrationBuilder.DropColumn(
                name: "SecretIsGreaterThan",
                table: "RoomPlayers");

            migrationBuilder.DropColumn(
                name: "SecretIsLessThan",
                table: "RoomPlayers");
        }
    }
}
