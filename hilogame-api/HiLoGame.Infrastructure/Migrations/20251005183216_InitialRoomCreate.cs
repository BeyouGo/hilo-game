using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HiLoGame.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialRoomCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Min = table.Column<int>(type: "int", nullable: false),
                    Max = table.Column<int>(type: "int", nullable: false),
                    Secret = table.Column<int>(type: "int", nullable: false),
                    Started = table.Column<bool>(type: "bit", nullable: false),
                    Finished = table.Column<bool>(type: "bit", nullable: false),
                    WinnerPlayerId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OwnerId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoomPlayers",
                columns: table => new
                {
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlayerId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomPlayers", x => new { x.RoomId, x.PlayerId });
                    table.ForeignKey(
                        name: "FK_RoomPlayers_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoomPlayers_RoomId_JoinedAt",
                table: "RoomPlayers",
                columns: new[] { "RoomId", "JoinedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_Name_CreatedAt",
                table: "Rooms",
                columns: new[] { "Name", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_OwnerId",
                table: "Rooms",
                column: "OwnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoomPlayers");

            migrationBuilder.DropTable(
                name: "Rooms");
        }
    }
}
