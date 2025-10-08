using HiLoGame.Application.Abstractions.Persistence;
using HiLoGame.Domain.Aggregates.Room.Entities;
using HiLoGame.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HiLoGame.Infrastructure.Repositories;

public class RoomRepository(HiLoGameDbContext context) : IRoomRepository
{
    public void Add(Room room)
    {
        context.Rooms.Add(room);
    }

    public async Task<Room?> GetByIdAsync(Guid roomId, CancellationToken ct)
    {
        return await context.Rooms
            .Include(s => s.Rules)
            .Include(s => s.Players) // required in order to have Players properly populated inside the Room aggregate
            .FirstOrDefaultAsync(s => s.Id == roomId, ct);
    }

    public IQueryable<Room> QueryAllByStatus(ERoomStatus? status)
    {
        if (!status.HasValue)
        {
            return context.Rooms;
        }

        return context.Rooms.Where(s => s.Status == status);
    }

    public async Task<IList<Room>> GetRoomsByPlayerIdAsync(string playerId, CancellationToken ct)
    {
        return await context.Rooms
            .Include(r => r.Players)
            .Where(r => r.Players.Any(p => p.PlayerId == playerId))
            .ToListAsync(ct);
    }

    public IQueryable<Room> QueryRoomsByOwnerIdAsync(string ownerId)
    {
        return context.Rooms
            .Include(r => r.Players)
            .Where(s => s.OwnerId == ownerId);
    }
}