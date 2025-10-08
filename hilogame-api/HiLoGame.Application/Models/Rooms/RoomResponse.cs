using System.Linq.Expressions;
using HiLoGame.Domain.Aggregates.Room.Entities;

namespace HiLoGame.Application.Models.Rooms;

public class RoomResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = "";
    public int Min { get; set; }
    public int Max { get; set; }
    public int MaxPlayerCount { get; set; }
    public string OwnerId { get; set; } = "";
    public string OwnerUsername { get; set; } = "";
    public string? WinnerPlayerId { get; init; }
    public string? WinnerPlayerUsername { get; init; }
    public int PlayerCount { get; init; }

    public List<RoomPlayerResponse> RoomPlayers { get; set; } = [];
    public ERoomStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Warning: Room should already be in-memory !
    /// </summary>
    /// <param name="room"></param>
    /// <returns></returns>
    public static RoomResponse From(Room room)
    {
        return RoomResponseExtensions.ToRoomResponseFunc.Invoke(room);
    }

}
public record RoomPlayerResponse(string Id, string Username, DateTime JoinedAt);

public sealed record LeaveAllResult(
    IList<Guid> RoomIds,
    IList<RoomResponse> RoomSummaries
);

public static class RoomResponseExtensions
{
    public static readonly Expression<Func<Room, RoomResponse>> ToRoomResponseExpression =
        r => new RoomResponse
        {
            Id = r.Id,
            CreatedAt = r.CreatedAt,
            Name = r.Name,
            Min = r.Rules.Min,
            Max = r.Rules.Max,
            MaxPlayerCount = r.Rules.MaxPlayers,
            Status = r.Status,
            WinnerPlayerId = r.WinnerPlayerId,
            WinnerPlayerUsername = r.WinnerPlayerUsername,
            OwnerId = r.OwnerId,
            OwnerUsername = r.OwnerUsername,
            PlayerCount = r.Players.Count, // server-side COUNT(*)
            RoomPlayers = r.Players.Select(rp =>  new RoomPlayerResponse(rp.PlayerId, rp.PlayerUsername, rp.JoinedAt)).ToList()
        };

    public static readonly Func<Room, RoomResponse> ToRoomResponseFunc = ToRoomResponseExpression.Compile();

    public static IQueryable<RoomResponse> SelectRoomResponse(this IQueryable<Room> query)
    {
        return query.Select(ToRoomResponseExpression);
    }
}