using HiLoGame.Application.Features.Authentication;
using HiLoGame.Shared.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HiLoGame.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> CreatePlayer([FromBody] RegisterPlayer.Command command, CancellationToken ct)
    {
        return Ok(await mediator.Send(command, ct));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginPlayer.Command command, CancellationToken ct)
    {
        var token = await mediator.Send(command, ct);
        return Ok(token);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetPlayerInfo(CancellationToken ct)
    {
        var player = await mediator.Send(new GetPlayerInfo.Query(User.GetId()), ct);
        return Ok(player);
    }
}
