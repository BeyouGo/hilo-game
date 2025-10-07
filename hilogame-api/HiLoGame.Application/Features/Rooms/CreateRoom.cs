using HiLoGame.Application.Common.Exceptions;
using HiLoGame.Application.Models.Rooms;
using HiLoGame.Application.Services;
using MediatR;

namespace HiLoGame.Application.Features.Rooms;

public class CreateRoom
{
    public record Command : IRequest<RoomResponse>
    { 
        public required string Name { get; set; }
        public string OwnerId { get; set; } = "";

    }

    public class Handler(IRoomService roomService) : IRequestHandler<Command, RoomResponse>
    {

        public async Task<RoomResponse> Handle(Command command, CancellationToken ct)
        {

            if (command == null || string.IsNullOrEmpty(command.Name))
            {
                throw new BadRequestException("Invalid room info");
            }

            var room = await roomService.CreateAsync(command.Name, command.OwnerId, ct);

            return room ?? throw new BadRequestException("User can't be created");
        }
    }
}