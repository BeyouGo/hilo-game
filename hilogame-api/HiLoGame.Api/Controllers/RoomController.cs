using HiLoGame.Application.Features.Rooms;
using HiLoGame.Domain.Aggregates.Room.Entities;
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

    // GET /rooms?status=awaitingPlayers&page=1&pageSize=10
    [HttpGet]
    public async Task<IActionResult> GetRooms([FromQuery] ERoomStatus status = ERoomStatus.AwaitingPlayers, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetRoomsPage.Query
        {
            Status = status,
            Page = page,
            PageSize = pageSize
        }, ct);

        return Ok(result);
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
