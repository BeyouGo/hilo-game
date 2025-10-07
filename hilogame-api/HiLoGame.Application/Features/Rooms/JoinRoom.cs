using HiLoGame.Application.Models.Rooms;
using HiLoGame.Application.Services;
using MediatR;

namespace HiLoGame.Application.Features.Rooms;

public class JoinRoom
{
    public class Command : IRequest<RoomResponse>
    {
        public string PlayerId { get; set; }
        public required Guid RoomId { get; set; }
    }

    public class Handler(IRoomService roomService) : IRequestHandler<Command, RoomResponse>
    {
        public async Task<RoomResponse> Handle(Command command, CancellationToken ct)
        {
            var room = await roomService.JoinAsync(command.RoomId, command.PlayerId, ct);
            return room ?? throw new InvalidOperationException("Player can't join the room");
        }
    }
}