using HiLoGame.Application.Abstractions.Realtime;
using HiLoGame.Application.Services;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace HiLoGame.Infrastructure.Realtime;

public class HiLoGameHub : Hub<IHiLoGameClient>
{
    private readonly IRoomService _rooms;
    private readonly IGameNotifier _gameNotifier;

    private static string ToRoomGroup(Guid roomId) => $"room:{roomId}";
    private static string ToCallerGroup(string playerId, Guid roomId) => $"player:{playerId}:room:{roomId}";

    public string GetId(ClaimsPrincipal user) => user.Claims.First(s => s.Type == ClaimTypes.NameIdentifier).Value;

    public HiLoGameHub(IRoomService rooms, IGameNotifier gameNotifier)
    {
        _rooms = rooms;
        _gameNotifier = gameNotifier;
    }

    public async Task JoinRoom(Guid roomId)
    {

        var playerId = GetId(Context.User);

        await Groups.AddToGroupAsync(Context.ConnectionId, ToRoomGroup(roomId));
        await Groups.AddToGroupAsync(Context.ConnectionId, ToCallerGroup(playerId, roomId));
        
        var roomResponse = await _rooms.JoinAsync(roomId, playerId, CancellationToken.None);

        await _gameNotifier.PlayerJoined(playerId, roomResponse);
    }

    public async Task LeaveRoom(Guid roomId)
    {

        var playerId = GetId(Context.User);

        await _rooms.LeaveAsync(roomId, playerId, CancellationToken.None);

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, ToRoomGroup(roomId));
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, ToCallerGroup(playerId, roomId));
    }

    public async Task StartGame(Guid roomId)
    {
        var playerId = GetId(Context.User);

        await _rooms.StartAsync(roomId, playerId, CancellationToken.None);
        await _gameNotifier.GameStarted(roomId);
    }

    public async Task MakeGuess(Guid roomId, int guess)
    {
        var playerId = GetId(Context.User);
        var guessResponse = await _rooms.MakeGuess(roomId, playerId, guess, CancellationToken.None);
        await _gameNotifier.GuessFeedback(roomId, playerId, guessResponse);
    }


    // Make sure we also clean up if the browser closes or the network drops
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var playerId = GetId(Context.User);
        if (!string.IsNullOrEmpty(playerId))
        {
            // Ask the domain which rooms this player was in, and leave them all.
            // Suggested contract: LeaveAllAsync returns the rooms that were affected.
            var left = await _rooms.LeaveAllAsync(playerId!, CancellationToken.None);

            // Remove this connection from each SignalR group (idempotent)
            foreach (var rid in left.RoomIds)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, ToRoomGroup(rid));
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, ToCallerGroup(playerId!, rid));

            }

            // Notify rooms/players as needed
            // Suggested notifier: PlayersDisconnected(playerId, left.RoomSummaries)
            await _gameNotifier.PlayersDisconnected(playerId!, left.RoomSummaries);
        }

        await base.OnDisconnectedAsync(exception);
    }


}