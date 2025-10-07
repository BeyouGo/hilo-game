using HiLoGame.Application.Models.Rooms;

namespace HiLoGame.Shared.Realtime;

public interface IHiLoGameClient
{
    Task GameStarted(Guid roomId);
    Task GameEnded(Guid roomId);
    Task RoomClosed(Guid roomId, RoomResponse room);
    Task GuessFeedback(Guid roomId, string playerId, GuessResponse feedback);
    Task PlayerJoined(string playerId, RoomResponse roomId);
    Task PlayerLeft(string playerId, RoomResponse room);
    Task PlayerDisconnected(string playerId, RoomResponse roomSummary);
}