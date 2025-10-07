using HiLoGame.Application.Models.Rooms;

namespace HiLoGame.Application.Abstractions.Realtime;

public interface IHiLoGameClient
{
    Task PlayerJoined(string playerId, RoomResponse roomId);
    Task PlayerLeft(string playerId, RoomResponse room);
    Task RoomClosed(Guid roomId, RoomResponse room);
    Task GameStarted(Guid roomId);
    Task GameEnded(Guid roomId);
    Task GuessFeedback(Guid roomId, string playerId, GuessResponse feedback);
    Task PlayerDisconnected(string playerId, RoomResponse roomSummary);
}