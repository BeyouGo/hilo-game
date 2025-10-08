using HiLoGame.Application.Abstractions.Persistence;
using HiLoGame.Application.Models;
using HiLoGame.Application.Models.Generic;
using HiLoGame.Application.Models.Rooms;
using HiLoGame.Domain.Aggregates.Room.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HiLoGame.Application.Features.Rooms;

public static class GetRoomsPage
{
    public sealed class Query : IRequest<PageResult<RoomResponse>>
    {
        public ERoomStatus? Status { get; init; } = ERoomStatus.AwaitingPlayers;
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 10;
    }

    public sealed class Handler(IUnitOfWork unitOfWork) : IRequestHandler<Query, PageResult<RoomResponse>>
    {
        private const int MaxPageSize = 100;

        public async Task<PageResult<RoomResponse>> Handle(Query query, CancellationToken ct)
        {
            var page = query.Page <= 0 ? 1 : query.Page;
            var pageSize = query.PageSize <= 0 ? 10 : Math.Min(query.PageSize, MaxPageSize);


            //if (!Enum.TryParse(query.Status, out ERoomStatus eStatus))
            //{
            //    eStatus = ERoomStatus.AwaitingPlayers;
            //}

            // Base query des rooms en attente
            var baseQuery = unitOfWork.RoomRepository
                .QueryAllByStatus(query.Status)
                .SelectRoomResponse()
                .AsNoTracking();

            // Total AVANT pagination
            var total = await baseQuery.CountAsync(ct);

            var pageItems = await baseQuery
                .OrderByDescending(r => r.CreatedAt)   
                .ThenBy(r => r.Id)                     
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PageResult<RoomResponse>
            {
                Items = pageItems,
                Total = total,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}