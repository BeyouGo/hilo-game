using HiLoGame.Application.Abstractions.Persistence;
using HiLoGame.Application.Abstractions.Realtime;
using HiLoGame.Application.Common.Exceptions;
using HiLoGame.Application.Models.Rooms;
using HiLoGame.Domain.Aggregates.Room.Entities;
using HiLoGame.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace HiLoGame.Application.Services;



public interface IRoomService
{
    Task<RoomResponse> CreateAsync(string name, string ownerId, CancellationToken ct);
    Task<RoomResponse> JoinAsync(Guid roomId, string playerId, CancellationToken ct);
    Task<RoomResponse> LeaveAsync(Guid roomId, string playerId, CancellationToken ct);
    Task<RoomResponse> StartAsync(Guid roomId, string playerId, CancellationToken ct);
    Task<GuessResponse> MakeGuess(Guid roomId, string playerId, int guess, CancellationToken ct);
    
    //TODO :
    // remove a player for all current room
    Task<LeaveAllResult> LeaveAllAsync(string playerId, CancellationToken none);
}

public class RoomService(IUnitOfWork unitOfWork, ISecretGenerator secretGenerator, IGameNotifier _notifier) : IRoomService
{

    public async Task<RoomResponse> CreateAsync(string name, string ownerId, CancellationToken ct)
    {
        var owner = await unitOfWork.PlayerRepository.GetById(ownerId, ct);

        if (owner == null)
        {
            throw new NotFoundException("Owner not found", ownerId);
        }

        var hasAwaitingPlayerRoom = await unitOfWork.RoomRepository.QueryRoomsByOwnerIdAsync(ownerId)
            .Where(s => s.Status == ERoomStatus.AwaitingPlayers)
            .AnyAsync(ct);

        if (hasAwaitingPlayerRoom)
        {
            throw new ForbiddenException("You cannot have more than one room awaiting players at the same time.");
        }

        var room = new Room(name, ownerId, owner.UserName!);
        
        unitOfWork.RoomRepository.Add(room);
        
        await unitOfWork.CommitAsync(ct);

        return RoomResponse.From(room);
    }

    public async Task<RoomResponse> JoinAsync(Guid roomId, string playerId, CancellationToken ct = default)
    {
        var player = await unitOfWork.PlayerRepository.GetById(playerId, ct);

        if (player == null)
        {
            throw new NotFoundException("Player not found", playerId);
        }

        var room = await unitOfWork.RoomRepository.GetByIdAsync(roomId, ct) ?? throw new NotFoundException("Room not found", roomId);
        room.AddPlayer(playerId, player.UserName!);
        await unitOfWork.CommitAsync(ct);

        var dto = RoomResponse.From(room);
        await _notifier.PlayerJoined(playerId, dto);
        return dto;
    }

    public async Task<RoomResponse> LeaveAsync(Guid roomId, string playerId, CancellationToken ct = default)
    {
        var player = await unitOfWork.PlayerRepository.GetById(playerId, ct);

        if (player == null)
        {
            throw new NotFoundException("Player not found", playerId);
        }

        var room = await unitOfWork.RoomRepository.GetByIdAsync(roomId, ct) ?? throw new NotFoundException("Room not found", roomId);
        room.RemovePlayer(playerId);

        await unitOfWork.CommitAsync(ct);


        var dto = RoomResponse.From(room);

        if (room.Status == ERoomStatus.Closed)
        {
            await _notifier.RoomClosed(roomId, dto);
        }
        else
        {
            await _notifier.PlayerLeft(roomId, playerId, dto);
        }

        return dto;
    }

    public async Task<RoomResponse> StartAsync(Guid roomId, string playerId, CancellationToken ct = default)
    {
        var room = await unitOfWork.RoomRepository.GetByIdAsync(roomId, ct);

        if (room == null)
        {
            throw new NotFoundException("No room found", roomId);
        }

        if (playerId != room.OwnerId)
        {
            throw new ForbiddenException("You can't start a game if you don't own the room");
        }


        var secret = secretGenerator.Generate(room.Rules);

        room.Start(secret);
        await unitOfWork.CommitAsync(ct);

        await _notifier.GameStarted(roomId);

        return RoomResponse.From(room);
    }

    public async Task<GuessResponse> MakeGuess(Guid roomId, string playerId, int guess, CancellationToken ct)
    {
        var room = await unitOfWork.RoomRepository.GetByIdAsync(roomId, ct);
        if (room == null)
        {
            throw new NotFoundException("No room found", roomId);
        }

        var outcome = room.MakeGuess(playerId, guess);
        await unitOfWork.CommitAsync(ct);

        if (outcome.IsWinningGuess)
        {
            //Game end when the first player has finished
            await _notifier.GameEnded(roomId);
        }

        return new GuessResponse()
        {
            Result = outcome.Result,
            PlayerId = playerId,
            Guess = guess,
            GuessedAt = outcome.GuessedAt,
            AttemptsAfterThisGuess = outcome.Attempts,
            RoomId = roomId,
            SecretIsBiggerThan = outcome.SecretIsGreaterThan,
            SecretIsLessThan = outcome.SecretIsLessThan,
            Finished = outcome.IsWinningGuess,
        };
    }


    public async Task<LeaveAllResult> LeaveAllAsync(string playerId, CancellationToken ct)
    {
        var player = await unitOfWork.PlayerRepository.GetById(playerId, ct);
        if (player == null)
        {
            throw new NotFoundException("Player not found", playerId);
        }
        
        var rooms = await unitOfWork.RoomRepository.GetRoomsByPlayerIdAsync(playerId, ct);
        if (rooms.Count == 0)
        {
            return new LeaveAllResult(Array.Empty<Guid>(), Array.Empty<RoomResponse>());
        }

        var affectedRoomIds = new List<Guid>(rooms.Count);
        var summaries = new List<RoomResponse>(rooms.Count);

        foreach (var room in rooms)
        {
            room.RemovePlayer(playerId);
            
            affectedRoomIds.Add(room.Id);
            
            summaries.Add(RoomResponse.From(room));
        }

        await unitOfWork.CommitAsync(ct);

        var leftTasks = summaries.Select(dto => _notifier.PlayerLeft(dto.Id, playerId, dto));
        var closedTasks = summaries.Where(r => r.PlayerCount == 0)
            .Select(r => _notifier.RoomClosed(r.Id, r));
        
        await Task.WhenAll(leftTasks.Concat(closedTasks));


        return new LeaveAllResult(affectedRoomIds, summaries);
    }
}

