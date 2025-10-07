using HiLoGame.Application.Abstractions.Persistence;
using HiLoGame.Infrastructure.Persistence;
using HiLoGame.Infrastructure.Repositories;

namespace HiLoGame.Infrastructure
{


    public class UnitOfWork(HiLoGameDbContext context) : IUnitOfWork
    {

        public IPlayerRepository PlayerRepository => new PlayerRepository(context);
        public IRoomRepository RoomRepository => new RoomRepository(context);

        public int Commit()
        {
            return context.SaveChanges();
        }

        public Task<int> CommitAsync(CancellationToken ct = default)
        {
            return context.SaveChangesAsync(ct);
        }

        public void Dispose()
        {
            context.Dispose();
        }
    }
}
