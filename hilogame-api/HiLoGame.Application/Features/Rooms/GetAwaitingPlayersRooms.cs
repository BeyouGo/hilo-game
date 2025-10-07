using HiLoGame.Application.Abstractions.Persistence;
using HiLoGame.Application.Models.Rooms;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HiLoGame.Application.Features.Rooms;

public class GetAwaitingPlayersRooms
{
    public class Query : IRequest<List<RoomResponse>> { }

    public class Handler(IUnitOfWork unitOfWork) : IRequestHandler<Query, List<RoomResponse>>
    {
        public async Task<List<RoomResponse>> Handle(Query command, CancellationToken ct)
        {
            return await unitOfWork.RoomRepository
                .QueryAllAwaitingPlayer()
                .SelectRoomResponse()
                .Take(50)
                .AsNoTracking()
                .ToListAsync(ct);
        }
    }
}