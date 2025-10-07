using HiLoGame.Application.Abstractions.Persistence;
using HiLoGame.Application.Common.Exceptions;
using HiLoGame.Application.Models.Players;
using MediatR;

namespace HiLoGame.Application.Features.Authentication;

public class GetPlayerInfo
{
    public record Query(string PlayerId) : IRequest<PlayerResponse>;
    public class Handler(IUnitOfWork unitOfWork) : IRequestHandler<Query, PlayerResponse>
    {
        public async Task<PlayerResponse> Handle(Query message, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(message.PlayerId))
            {
                throw new BadRequestException("Invalid playerId");
            }

            var player = await unitOfWork.PlayerRepository.GetById(message.PlayerId, ct);
            if (player == null)
            {
                throw new NotFoundException($"No user found",  message.PlayerId);
            }

            return PlayerResponse.From(player);
        }
    }
}