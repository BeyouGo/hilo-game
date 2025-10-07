using HiLoGame.Domain.Aggregates.Player;

namespace HiLoGame.Infrastructure.Repositories;

public interface IPlayerRepository
{
    void Create(Player user);
    Task<Player?> GetById(string id, CancellationToken ct);
    Task<Player?> GetByUsername(string id, CancellationToken ct);
    Task<bool> AnyByUsername(string username, CancellationToken ct);
}