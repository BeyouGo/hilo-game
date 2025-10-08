using HiLoGame.Domain.Aggregates.Room.Entities;

namespace HiLoGame.Application.Abstractions.Persistence;

public interface IRoomRepository
{ 
    void Add(Room room);
    Task<Room?> GetByIdAsync(Guid roomId, CancellationToken ct);
    IQueryable<Room> QueryAllByStatus(ERoomStatus? status);
    Task<IList<Room>> GetRoomsByPlayerIdAsync(string playerId, CancellationToken ct);
    IQueryable<Room> QueryRoomsByOwnerIdAsync(string owner);
}