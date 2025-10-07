using HiLoGame.Application.Abstractions.Realtime;
using HiLoGame.Application.Models.Rooms;
using Microsoft.AspNetCore.SignalR;

namespace HiLoGame.Infrastructure.Realtime;

public class GameNotifier : IGameNotifier
{
    private readonly IHubContext<HiLoGameHub, IHiLoGameClient> _hub;

    public GameNotifier(IHubContext<HiLoGameHub, IHiLoGameClient> hub) => _hub = hub;

    private static string ToRoomGroup(Guid roomId) => $"room:{roomId}";
    private static string ToCallerGroup(string playerId, Guid roomId) => $"player:{playerId}:room:{roomId}"; //notify only the player

    public Task RoomClosed(Guid roomId, RoomResponse room) =>
        _hub.Clients.Group(ToRoomGroup(roomId)).RoomClosed(roomId, room);
    

    public Task GameStarted(Guid roomId) =>
        _hub.Clients.Group(ToRoomGroup(roomId)).GameStarted(roomId);

    public Task GameEnded(Guid roomId) =>
        _hub.Clients.Group(ToRoomGroup(roomId)).GameEnded(roomId);

    public Task GuessFeedback(Guid roomId, string playerId, GuessResponse feedback) =>
        _hub.Clients.Group(ToCallerGroup(playerId, roomId)).GuessFeedback(roomId, playerId, feedback);



    public Task PlayerJoined(string playerId, RoomResponse room) =>
        _hub.Clients.Group(ToRoomGroup(room.Id)).PlayerJoined(playerId, room);


    public Task PlayersDisconnected(string playerId, IList<RoomResponse> leftRoomSummaries)
    {
        if (leftRoomSummaries.Count == 0)
            return Task.CompletedTask;

        // Preferred: dedicated event
        var sends = leftRoomSummaries
            .Select(s => _hub.Clients.Group(ToRoomGroup(s.Id))
                .PlayerDisconnected(playerId, s));

        return Task.WhenAll(sends);

        // Fallback if you don't have PlayerDisconnected on IHiLoGameClient:
        // var sends = leftRoomSummaries
        //     .Select(s => _hub.Clients.Group(ToRoomGroup(s.RoomId))
        //         .PlayerLeft(playerId, new RoomResponse
        //         {
        //             Id = s.RoomId,
        //             Name = s.Name,
        //             Started = s.Started,
        //             Finished = s.Finished,
        //             OwnerId = s.OwnerId,
        //             PlayerCount = s.PlayerCount,
        //             Players = Array.Empty<RoomPlayerResponse>() // or omit if your DTO allows
        //         }));
        // return Task.WhenAll(sends);
    }


    public Task PlayerLeft(Guid roomId, string playerId, RoomResponse room)
    {
        return _hub.Clients.Group(ToRoomGroup(roomId)).PlayerLeft(playerId, room);
    }

    

}