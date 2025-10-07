using HiLoGame.Infrastructure.Repositories;

namespace HiLoGame.Application.Abstractions.Persistence;

public interface IUnitOfWork : IDisposable
{
    public IPlayerRepository PlayerRepository { get; }
    public IRoomRepository RoomRepository { get; }

    int Commit();
    Task<int> CommitAsync(CancellationToken ct = default);
}