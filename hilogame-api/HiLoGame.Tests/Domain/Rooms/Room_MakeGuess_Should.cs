using FluentAssertions;
using HiLoGame.Domain.Aggregates.Room.Entities;
using HiLoGame.Domain.Common;

namespace HiLoGame.Tests.Domain.Rooms;

public class Room_MakeGuess_Should
{
    // --- Helpers -------------------------------------------------------------

    private static Room NewAwaitingRoomWithPlayers()
    {
        var room = new Room(name: "Test", ownerPlayerId: "owner", ownerUsername: "owner");
        room.AddPlayer("p1", "alice");
        room.AddPlayer("p2", "bob");
        return room;
    }

    private static (Room room, int secret) NewStartedRoomWithPlayers()
    {
        var room = NewAwaitingRoomWithPlayers();
        var secret = 7; // guaranteed in-range
        room.Start(secret);
        return (room, secret);
    }

    // --- Guard clauses -------------------------------------------------------

    [Fact]
    public void Throw_If_Not_Started()
    {
        var room = NewAwaitingRoomWithPlayers();

        Action act = () => room.MakeGuess("p1", 42);

        act.Should().Throw<DomainException>()
            .WithMessage("*not yet started*");
    }

    [Fact]
    public void Throw_If_Player_Not_In_Room()
    {
        var (room, _) = NewStartedRoomWithPlayers();

        Action act = () => room.MakeGuess("ghost", 42);

        act.Should().Throw<DomainException>()
            .WithMessage("*does not belong*");
    }

    [Fact]
    public void Throw_If_Player_Not_Playing()
    {
        var (room, _) = NewStartedRoomWithPlayers();

        // simulate a player that already left mid-game
        var left = room.Players.First(p => p.PlayerId == "p1");
        left.Status = ERoomPlayerStatus.Left;

        Action act = () => room.MakeGuess("p1", 42);

        act.Should().Throw<DomainException>()
            .WithMessage("*already finish*"); // your message says "finish its game"
    }

    [Fact]
    public void Throw_If_Guess_Out_Of_Range()
    {
        var (room, _) = NewStartedRoomWithPlayers();

        Action low = () => room.MakeGuess("p1", room.Rules.Min - 1);
        Action high = () => room.MakeGuess("p1", room.Rules.Max + 1);

        low.Should().Throw<DomainException>().WithMessage("*range*");
        high.Should().Throw<DomainException>().WithMessage("*range*");
    }

    // --- TooLow / TooBig paths ----------------------------------------------

    [Fact]
    public void TooLow_Increments_Attempts_Sets_Timestamps_And_Tightens_LowerBound()
    {
        var (room, secret) = NewStartedRoomWithPlayers();
        var guess = secret - 3;

        var before = DateTime.UtcNow;
        var outcome = room.MakeGuess("p1", guess);
        var after = DateTime.UtcNow;

        outcome.Result.Should().Be(EGuessResult.TooLow);
        outcome.Guess.Should().Be(guess);
        outcome.Attempts.Should().Be(1);
        outcome.IsWinningGuess.Should().BeFalse();

        var p1 = room.Players.First(p => p.PlayerId == "p1");
        p1.Attempts.Should().Be(1);
        p1.FirstGuessAt.Should().NotBeNull();
        p1.LastGuessAt.Should().NotBeNull();
        p1.FirstGuessAt!.Value.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        p1.LastGuessAt!.Value.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);

        // bounds updated
        p1.SecretIsGreaterThan.Should().Be(guess);
        p1.SecretIsLessThan.Should().Be(room.Rules.Max + 1);

        // outcome bounds echo the player's current interval
        outcome.SecretIsGreaterThan.Should().Be(p1.SecretIsGreaterThan);
        outcome.SecretIsLessThan.Should().Be(p1.SecretIsLessThan);
    }

    [Fact]
    public void TooBig_Increments_Attempts_Sets_Timestamps_And_Tightens_UpperBound()
    {
        var (room, secret) = NewStartedRoomWithPlayers();
        var guess = secret + 4;

        var outcome = room.MakeGuess("p1", guess);

        outcome.Result.Should().Be(EGuessResult.TooBig);
        outcome.Guess.Should().Be(guess);
        outcome.Attempts.Should().Be(1);
        outcome.IsWinningGuess.Should().BeFalse();

        var p1 = room.Players.First(p => p.PlayerId == "p1");
        p1.Attempts.Should().Be(1);
        p1.SecretIsLessThan.Should().Be(guess);
        p1.SecretIsGreaterThan.Should().Be(room.Rules.Min - 1);

        outcome.SecretIsGreaterThan.Should().Be(p1.SecretIsGreaterThan);
        outcome.SecretIsLessThan.Should().Be(p1.SecretIsLessThan);
    }

    [Fact]
    public void LowerBound_Is_Monotonic_NonDecreasing()
    {
        var (room, secret) = NewStartedRoomWithPlayers();
        var g1 = secret - 1; // low
        var g2 = secret - 2; // even lower => should NOT reduce the lower bound

        room.MakeGuess("p1", g1);
        var lbAfterG1 = room.Players.First(p => p.PlayerId == "p1").SecretIsGreaterThan;

        room.MakeGuess("p1", g2);
        var lbAfterG2 = room.Players.First(p => p.PlayerId == "p1").SecretIsGreaterThan;

        lbAfterG2.Should().Be(lbAfterG1); // stays at the max of previous and new guess
    }

    [Fact]
    public void UpperBound_Is_Monotonic_NonIncreasing()
    {
        var (room, secret) = NewStartedRoomWithPlayers();
        var g1 = secret + 1; // high
        var g2 = secret + 2; // even higher → should NOT increase the upper bound

        room.MakeGuess("p1", g1);
        var ubAfterG1 = room.Players.First(p => p.PlayerId == "p1").SecretIsLessThan;

        room.MakeGuess("p1", g2);
        var ubAfterG2 = room.Players.First(p => p.PlayerId == "p1").SecretIsLessThan;

        ubAfterG2.Should().Be(ubAfterG1); // stays at the min of previous and new guess
    }

    [Fact]
    public void FirstGuessAt_Is_Set_Once_LastGuessAt_Updates()
    {
        var (room, secret) = NewStartedRoomWithPlayers();

        room.MakeGuess("p1", secret - 1);
        var first = room.Players.First(p => p.PlayerId == "p1").FirstGuessAt;
        var last1 = room.Players.First(p => p.PlayerId == "p1").LastGuessAt;

        room.MakeGuess("p1", secret + 1);
        var firstAgain = room.Players.First(p => p.PlayerId == "p1").FirstGuessAt;
        var last2 = room.Players.First(p => p.PlayerId == "p1").LastGuessAt;

        firstAgain.Should().Be(first);         // unchanged
        last2.Should().BeOnOrAfter(last1!.Value); // moved forward (or equal in very fast tests)
    }

    // --- Win path ------------------------------------------------------------

    [Fact]
    public void Win_Sets_Winner_Finishes_All_Players_And_Returns_Win_Outcome()
    {
        var (room, secret) = NewStartedRoomWithPlayers();

        var outcome = room.MakeGuess("p1", secret);

        outcome.Result.Should().Be(EGuessResult.Win);
        outcome.IsWinningGuess.Should().BeTrue();
        outcome.Guess.Should().Be(secret);
        outcome.Attempts.Should().Be(1);
        outcome.SecretIsGreaterThan.Should().Be(secret); // per your Win return: (guess, guess)
        outcome.SecretIsLessThan.Should().Be(secret);

        room.Status.Should().Be(ERoomStatus.Finished);
        room.WinnerPlayerId.Should().Be("p1");
        room.Players.Should().OnlyContain(p => p.Status == ERoomPlayerStatus.Finished);
        room.Players.First(p => p.PlayerId == "p1").Attempts.Should().Be(1);
    }

    [Fact]
    public void After_Win_The_Winner_Cannot_Guess_Again()
    {
        var (room, secret) = NewStartedRoomWithPlayers();

        room.MakeGuess("p1", secret); // finishes game
        Action again = () => room.MakeGuess("p1", secret);

        again.Should().Throw<DomainException>()
             .WithMessage("*finish*");
    }
}