using FluentAssertions;
using HiLoGame.Domain.Aggregates.Room.Entities;
using HiLoGame.Domain.Common;
using HiLoGame.Domain.Tests.Builders;

namespace HiLoGame.Tests.Domain.Rooms;

public class Room_Create_And_RemovePlayer_Should
{

    [Fact]
    public void Removing_NonExisting_Player_Is_Idempotent()
    {
        var room = new RoomBuilder().Build();

        room.Invoking(r => r.RemovePlayer("ghost")).Should().NotThrow();

        room.Status.Should().Be(ERoomStatus.AwaitingPlayers);
        room.Players.Should().ContainSingle(p => p.PlayerId == "owner");
    }

    [Fact]
    public void Awaiting_NonOwner_Leaves_Removes_Player()
    {
        var room = new RoomBuilder().Build();
        room.AddPlayer("p1", "alice");

        room.RemovePlayer("p1");

        room.Status.Should().Be(ERoomStatus.AwaitingPlayers);
        room.Players.Should().ContainSingle(p => p.PlayerId == "owner");
    }


    [Fact]
    public void Awaiting_Owner_Leaves_Closes_Immediately()
    {
        var room = new RoomBuilder().Build();
        room.AddPlayer("p1", "alice");

        room.RemovePlayer("owner");

        room.Status.Should().Be(ERoomStatus.Closed);

        room.Players.Should().AllSatisfy(s => s.Status.Should().Be(ERoomPlayerStatus.Left));

        room.Players.Should().Contain(p => p.PlayerId == "owner");
        room.Players.Should().Contain(p => p.PlayerId == "p1");

    }


    [Fact]
    public void Awaiting_Last_Player_Leaves_Closes_Room()
    {
        // Start with owner only, then remove owner → closed
        var room = new RoomBuilder().Build();

        room.RemovePlayer("owner");

        room.Status.Should().Be(ERoomStatus.Closed);
    }


    [Fact]
    public void Started_Player_Leaves_Is_Marked_Left_But_Room_Continues()
    {
        var room = new RoomBuilder().Build();
        room.AddPlayer("p1", "alice");
        room.AddPlayer("p2", "bob");
        
        room.Start(5);

        room.RemovePlayer("p1");

        var p1 = room.Players.First(p => p.PlayerId == "p1");
        p1.Status.Should().Be(ERoomPlayerStatus.Left);
        room.Status.Should().Be(ERoomStatus.Started);

        room.MakeGuess("p2", 5);
    }

    [Fact]
    public void Started_Player_Leaves_ShouldNot_Make_Guess_But_Room_Continues()
    {
        var room = new RoomBuilder().Build();
        room.AddPlayer("p1", "alice");
        room.AddPlayer("p2", "bob");

        room.Start(5);

        room.RemovePlayer("p1");

        var act2 = () => room.MakeGuess("p1", 4);
        act2.Should().Throw<DomainException>();

        var act = () => room.MakeGuess("p2", 4);
        act.Should().NotThrow();

        room.Status.Should().Be(ERoomStatus.Started);
    }


    [Fact]
    public void Finished_Or_Closed_Remove_Is_NoOp_On_State()
    {
        // Finish the game first
        var room = new RoomBuilder().Build();

        var secret = 5;

        room.AddPlayer("p1", "alice");
        room.Start(secret);
        room.MakeGuess("p1", secret); // finishes game

        room.Status.Should().Be(ERoomStatus.Finished);

        // Removing any player in Finished should not change room state
        room.RemovePlayer("p1");
        room.Status.Should().Be(ERoomStatus.Finished);

        room.RemovePlayer("owner"); 
        room.Status.Should().Be(ERoomStatus.Finished); 
    }


    [Fact]
    public void Finished_Or_Closed_Remove_Is_NoOp_On_PlayerState()
    {
        // Finish the game first
        var room = new RoomBuilder().Build();

        var secret = 5;

        room.AddPlayer("p1", "alice");
        room.Start(secret);
        room.MakeGuess("p1", secret); // finishes game

        room.Status.Should().Be(ERoomStatus.Finished);

        // Removing any player in Finished should not change playerRoom state
        room.RemovePlayer("p1");
        room.RemovePlayer("owner");

        room.Players.Should().HaveCount(2);
        room.Players.Should().AllSatisfy(p => p.Status.Should().Be(ERoomPlayerStatus.Finished));
    }
}