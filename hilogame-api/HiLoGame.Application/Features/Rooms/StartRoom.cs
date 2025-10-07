using HiLoGame.Application.Abstractions.Persistence;
using HiLoGame.Application.Models.Rooms;
using HiLoGame.Application.Services;
using MediatR;

namespace HiLoGame.Application.Features.Rooms;

public class StartRoom
{
    public class Command : IRequest<RoomResponse>
    {
        public string PlayerId { get; set; } = "";
        public required Guid RoomId { get; set; }
    }

    public class Handler(IUnitOfWork unitOfWork, IRoomService roomService) : IRequestHandler<Command, RoomResponse>
    {
        public async Task<RoomResponse> Handle(Command command, CancellationToken ct)
        {

            var room = await unitOfWork.RoomRepository.GetByIdAsync(command.RoomId, ct);
            if (room == null)
            {
                throw new KeyNotFoundException($"No room with id {command.RoomId}");
            }

            var roomResponse = await roomService.StartAsync(command.RoomId, command.PlayerId, ct);
            return roomResponse ?? throw new InvalidOperationException("Player can't join the room");
        }
    }
}