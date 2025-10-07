using HiLoGame.Domain.Aggregates.Player;
using HiLoGame.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HiLoGame.Infrastructure.Repositories;

public class PlayerRepository(HiLoGameDbContext context) : IPlayerRepository
{

    public void Create(Player player)
    {
        context.Players.Add(player);
    }

    public async Task<Player?> GetById(string id, CancellationToken ct)
    {
        return await context.Players.FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<Player?> GetByUsername(string usename, CancellationToken ct)
    {
        return await context.Players.FirstOrDefaultAsync(s => s.UserName == usename, ct);
    }

    public async Task<bool> AnyByUsername(string username, CancellationToken ct)
    {
        return await context.Players.AnyAsync(s => s.UserName == username, ct);
    }
}