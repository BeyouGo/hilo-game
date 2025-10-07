using FluentAssertions;
using HiLoGame.Domain.Aggregates.Room.Entities;
using HiLoGame.Domain.Common;
using HiLoGame.Domain.Tests.Builders;

namespace HiLoGame.Tests.Domain.Rooms;

public class Room_Create_And_AddPlayer_Should
{
    [Fact]
    public void Create_With_Owner_In_AwaitingPlayers_And_DefaultRules()
    {
        var room = new RoomBuilder().Build();

        room.Status.Should().Be(ERoomStatus.AwaitingPlayers);
        room.OwnerId.Should().Be("owner");
        room.OwnerUsername.Should().Be("owner");

        room.Players.Should().ContainSingle(p => p.PlayerId == "owner");
        
        room.Rules.Min.Should().BeLessThan(room.Rules.Max);

        room.CreatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void AddPlayer_When_Awaiting_Should_Add_Unique_Player()
    {
        var room = new RoomBuilder().Build();

        room.AddPlayer("p1", "alice");
        room.AddPlayer("p1", "alice"); // idempotent

        room.Players.Should().ContainSingle(p => p.PlayerId == "p1");
    }

    [Fact]
    public void AddPlayer_When_MaxPlayerReached_Should_Throw()
    {
        var room = new RoomBuilder().Build();

        var maxPlayers = room.Rules.MaxPlayers; 

        for (int i = 0; i < maxPlayers - 1; i++) // -1 because the owner is already in the room
        {
            room.AddPlayer("p" + i, "name_" + i);
        }

        room.Players.Should().HaveCount(maxPlayers );
        
        var act = () => room.AddPlayer("one_too_many", "bob");
        
        act.Should().Throw<DomainException>().WithMessage("*limit exceeded*");
    }

    [Fact]
    public void AddPlayer_When_Not_Awaiting_Should_Throw()
    {
        var room = new RoomBuilder().Build();
        room.Start(5);

        var act = () => room.AddPlayer("p2", "bob");

        act.Should().Throw<DomainException>().WithMessage("*not awaiting*");
    }
}