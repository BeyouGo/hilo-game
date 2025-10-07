using HiLoGame.Application.Models.Rooms;

namespace HiLoGame.Application.Abstractions.Realtime;

public interface IGameNotifier
{
    Task PlayerJoined(string playerId, RoomResponse room);
    Task PlayerLeft(Guid roomId, string playerId, RoomResponse room);
    Task RoomClosed(Guid roomId, RoomResponse room);
    Task GameStarted(Guid roomId);
    Task GameEnded(Guid roomId);
    Task GuessFeedback(Guid roomId, string playerId, GuessResponse guessResponse);
    Task PlayersDisconnected(string playerId, IList<RoomResponse> leftRoomSummaries);
}