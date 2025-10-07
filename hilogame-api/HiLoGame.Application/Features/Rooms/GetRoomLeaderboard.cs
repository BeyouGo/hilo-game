using HiLoGame.Application.Abstractions.Persistence;
using HiLoGame.Application.Common.Exceptions;
using HiLoGame.Application.Models.Rooms;
using HiLoGame.Domain.Aggregates.Room.Entities;
using MediatR;

namespace HiLoGame.Application.Features.Rooms;

public class GetRoomLeaderboard
{
    public class Query : IRequest<RoomLeaderboardResponse>
    {
        public Guid RoomId { get; set; }
        public string PlayerId { get; set; }
    }

    public class Handler(IUnitOfWork unitOfWork) : IRequestHandler<Query, RoomLeaderboardResponse>
    {
        public async Task<RoomLeaderboardResponse> Handle(Query query, CancellationToken ct)
        {

            var room = await unitOfWork.RoomRepository.GetByIdAsync(query.RoomId, ct);
            if (room == null)
            {
                throw new NotFoundException("No room found", query.RoomId);
            }

            if (room.Players.All(p => p.PlayerId != query.PlayerId))
            {
                throw new ForbiddenException("You can only see leaderboard of your own game");
            }

            var entries = room.ComputeLeaderboardEntries();

            var winner = room.Players.First(p => p.PlayerId == room.WinnerPlayerId!)!; // Domain verified winner presence

            var ordered = entries
                .OrderBy(e => e.PlayerId == winner.PlayerId ? 0 : 1) // winner first
                .ThenByDescending(e => e.Finished)                    // finished before leavers
                .ThenBy(e => e.Width)                    // smaller interval first
                .ToList();

            return new RoomLeaderboardResponse(
                room.Id,
                winner.PlayerId,
                winner.PlayerUsername,
                winner.LastGuessAt ?? DateTime.UtcNow,
                ordered.Select(e => new RoomLeaderboardEntry(
                    e.PlayerId, e.PlayerUsername, e.Finished, e.Attempts,
                    e.FirstGuessAt, e.LastGuessAt, e.LowerBound, e.UpperBound, e.Width)).ToList());

        }
    }
}