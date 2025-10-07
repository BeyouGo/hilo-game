using HiLoGame.Application.Features.Rooms;
using HiLoGame.Shared.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HiLoGame.Api.Controllers;

[ApiController, Route("rooms"), Authorize]
public class RoomController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoom.Command command, CancellationToken ct)
    {
        command.OwnerId = User.GetId();
        return Ok(await mediator.Send(command, ct));
    }

    [HttpGet]
    public async Task<IActionResult> GetPendingRooms(CancellationToken ct)
    {
        var dto = await mediator.Send(new GetAwaitingPlayersRooms.Query(), ct);
        return Ok(dto);
    }


    [HttpGet("{roomId:guid}/leaderboard")]
    public async Task<IActionResult> GetRoomLeaderboard(Guid roomId, CancellationToken ct)
    {
        return Ok(await mediator.Send(new GetRoomLeaderboard.Query()
        {
            RoomId = roomId,
            PlayerId = User.GetId(),
        }, ct));
    }
}
