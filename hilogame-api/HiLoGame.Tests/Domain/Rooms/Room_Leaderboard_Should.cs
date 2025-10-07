using FluentAssertions;
using HiLoGame.Domain.Aggregates.Room.Entities;
using HiLoGame.Domain.Common;

namespace HiLoGame.Tests.Domain.Rooms;

public class Room_Leaderboard_Should
{
    // --- helpers -------------------------------------------------------------

    private static Room NewAwaitingRoomWithPlayers()
    {
        var room = new Room("Test", "owner", "owner");
        room.AddPlayer("p1", "alice");
        room.AddPlayer("p2", "bob");
        room.AddPlayer("p3", "charlie");
        return room;
    }

    private static (Room room, int secret) NewStartedRoomWithPlayers()
    {
        var room = NewAwaitingRoomWithPlayers();
        var secret = 5;
        room.Start(secret);
        return (room, secret);
    }

    // --- guards --------------------------------------------------------------

    [Fact]
    public void Throw_If_Room_Not_Finished()
    {
        var (room, _) = NewStartedRoomWithPlayers();

        Action act = () => room.ComputeLeaderboardEntries();

        act.Should().Throw<DomainException>()
           .WithMessage("*Leaderboard only available for finished games*");
    }

    [Fact]
    public void Throw_If_Winner_Not_Set_Even_Though_Finished()
    {
        var (room, _) = NewStartedRoomWithPlayers();

        // Force an invalid state: Finished but no Winner (simulate a bug)
        room.GetType().GetProperty(nameof(Room.Status))!.SetValue(room, ERoomStatus.Finished);
        room.GetType().GetProperty(nameof(Room.WinnerPlayerId))!.SetValue(room, null);

        Action act = () => room.ComputeLeaderboardEntries();

        act.Should().Throw<DomainException>()
           .WithMessage("*Winner not set although room is finished*");
    }

    // --- happy path ----------------------------------------------------------

    [Fact]
    public void Produce_Entries_With_Correct_Bounds_Width_And_Flags()
    {
        var (room, secret) = NewStartedRoomWithPlayers();

        // p2 narrows interval: one too-low then one too-big
        var lowGuess = secret - 3;
        var highGuess = secret + 7;

        room.MakeGuess("p2", lowGuess);   // TooLow -> SecretIsGreaterThan = lowGuess
        room.MakeGuess("p2", highGuess);  // TooBig -> SecretIsLessThan   = highGuess

        // p3 leaves mid-game (should remain Finished=false at the end)
        room.RemovePlayer("p3");

        // p1 wins
        room.MakeGuess("p1", secret);

        // compute
        var entries = room.ComputeLeaderboardEntries();

        // --- common checks
        entries.Should().HaveCount(4); // 3 added players + owner
        var e1 = entries.Single(e => e.PlayerId == "p1"); // winner
        var e2 = entries.Single(e => e.PlayerId == "p2");
        var e3 = entries.Single(e => e.PlayerId == "p3");

        // Winner: width forced to 0 (even if bounds were not updated before the win)
        e1.Finished.Should().BeTrue();
        e1.Attempts.Should().Be(1);
        e1.Width.Should().Be(0);

        // p2: bounds formula lower = (SecretIsGreaterThan + 1), upper = (SecretIsLessThan - 1).
        // After lowGuess & highGuess we expect:
        //   lower = lowGuess + 1
        //   upper = highGuess - 1
        //   width = max(0, upper - lower) = (highGuess - 1) - (lowGuess + 1) = highGuess - lowGuess - 2
        e2.Finished.Should().BeTrue(); // winner caused all Playing -> Finished
        e2.LowerBound.Should().Be(lowGuess + 1);
        e2.UpperBound.Should().Be(highGuess - 1);
        e2.Width.Should().Be(Math.Max(0, (highGuess - 1) - (lowGuess + 1)));

        // p3: left mid-game -> not finished
        e3.Finished.Should().BeFalse();
    }

    [Fact]
    public void Preserve_Attempts_And_Timestamps()
    {
        var (room, secret) = NewStartedRoomWithPlayers();

        var before = DateTime.UtcNow;
        room.MakeGuess("p2", secret - 1);
        room.MakeGuess("p2", secret + 1);
        var middle = DateTime.UtcNow;

        // p1 wins to finish the room
        room.MakeGuess("p1", secret);
        var after = DateTime.UtcNow;

        var entries = room.ComputeLeaderboardEntries();
        var e2 = entries.First(e => e.PlayerId == "p2");

        e2.Attempts.Should().Be(2);
        e2.FirstGuessAt.Should().NotBeNull();
        e2.LastGuessAt.Should().NotBeNull();

        e2.FirstGuessAt!.Value.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        e2.LastGuessAt!.Value.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Width_Never_Negative()
    {
        var (room, secret) = NewStartedRoomWithPlayers();

        // p2 intentionally crosses bounds tightly:
        // guess low just below secret, and high just above secret
        room.MakeGuess("p2", secret - 1);
        room.MakeGuess("p2", secret + 1);

        room.MakeGuess("p1", secret); // finish

        var e2 = room.ComputeLeaderboardEntries()
                     .Single(e => e.PlayerId == "p2");

        e2.Width.Should().BeGreaterThanOrEqualTo(0);
    }
}