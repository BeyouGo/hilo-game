using HiLoGame.Domain.Aggregates.Room.Outcomes;
using HiLoGame.Domain.Aggregates.Room.ValueObjects;
using HiLoGame.Domain.Common;

namespace HiLoGame.Domain.Aggregates.Room.Entities;

public class Room
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = default!;
    public GameRules Rules { get; private set; }
    public int Secret { get; private set; } // <= secret number
    public ERoomStatus Status { get; set; }
    public string? WinnerPlayerId { get; private set; }
    public string OwnerId { get; private set; }
    public string OwnerUsername { get; private set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    //RowVersion is added directly from RoomConfig.cs.

    private readonly List<RoomPlayer> _players = new();
    public virtual IReadOnlyCollection<RoomPlayer> Players => _players.AsReadOnly();

    private Room() { } // EF

    public Room(string name, string ownerPlayerId, string ownerUsername)
    {

        Name = !string.IsNullOrWhiteSpace(name) ? name : throw new DomainException("Name required.");
        OwnerId = ownerPlayerId ?? throw new DomainException("Owner required.");
        OwnerUsername = ownerUsername ?? throw new DomainException("Owner username required.");
        Rules = GameRules.CreateDefault();
        Status = ERoomStatus.AwaitingPlayers;
        CreatedAt = DateTime.UtcNow;

        AddPlayer(ownerPlayerId, ownerUsername);
    }

    public void AddPlayer(string playerId, string playerUsername)
    {
        if (Status != ERoomStatus.AwaitingPlayers)
        {
            throw new DomainException("The room is not awaiting player.");
        }

        if (Players.Any(p => p.PlayerId == playerId))
        {
            return;
        }

        Rules.EnsurePlayersAllowed(_players.Count + 1);

        _players.Add(new RoomPlayer(Id, playerId, playerUsername));
    }


    public void Start(int secret)
    {
        if (Status != ERoomStatus.AwaitingPlayers)
        {
            throw new DomainException("Room must be in AwaitingPlayer status to start the game");
        }

        if (_players.Count == 0)
        {
            throw new DomainException("Can't start a game without players");
        }

        Rules.EnsureGuessInRange(secret);


        foreach (var rp in _players)
        {
            rp.Status = ERoomPlayerStatus.Playing;
            rp.SecretIsGreaterThan = Rules.Min - 1;
            rp.SecretIsLessThan = Rules.Max + 1;
            rp.FirstGuessAt = null;
            rp.LastGuessAt = null;
            rp.Attempts = 0; 
        }
        

        Secret = secret;
        Status = ERoomStatus.Started;
        WinnerPlayerId = null;
    }

    public GuessOutcome MakeGuess(string playerId, int guess)
    {

        if (Status != ERoomStatus.Started)
        {
            throw new DomainException("The room has not yet started the game or is finish / closed!");
        }

        var rp = _players.FirstOrDefault(s => s.PlayerId == playerId);
        if (rp == null)
        {
            throw new DomainException("The player does not belong to this game");
        }

        if (rp.Status != ERoomPlayerStatus.Playing)
        {
            throw new DomainException("The player has already finish its game");
        }

        Rules.EnsureGuessInRange(guess);

        var now = DateTime.UtcNow;
        rp.FirstGuessAt ??= now;
        rp.LastGuessAt = now;

        rp.Attempts += 1;

        if (guess == Secret)
        {
            //Should end the game for all players

            Status = ERoomStatus.Finished;
            WinnerPlayerId = playerId;

            foreach (var player in _players.Where(s => s.Status == ERoomPlayerStatus.Playing))
            {
                player.Status = ERoomPlayerStatus.Finished;
            }

            return new GuessOutcome(EGuessResult.Win,
                rp.LastGuessAt.Value,
                guess,
                rp.Attempts,
                true, 
                guess, 
                guess) ;
        }


        if (guess < Secret)
        {
            rp.SecretIsGreaterThan = Math.Max(rp.SecretIsGreaterThan, guess);
            return new GuessOutcome(EGuessResult.TooLow,
                rp.LastGuessAt.Value,
                guess,
                rp.Attempts,
                false,
                rp.SecretIsGreaterThan,
                rp.SecretIsLessThan);
        }
        else
        {
            rp.SecretIsLessThan = Math.Min(rp.SecretIsLessThan, guess);
             return new GuessOutcome(EGuessResult.TooBig,
                 rp.LastGuessAt.Value,
                guess,
                rp.Attempts,
                false, 
                rp.SecretIsGreaterThan, 
                rp.SecretIsLessThan) ;
        }
    }

    public void RemovePlayer(string playerId)
    {
        var p = _players.FirstOrDefault(x => x.PlayerId == playerId);
        if (p is null)
        {
            //Idempotent
            return;
        }
    
        if (Status is (ERoomStatus.Closed or ERoomStatus.Finished))
        {
            // Do nothing; We keep the RoomPlayer in DB for the record.
            return;
        }

        if (Status == ERoomStatus.AwaitingPlayers)
        {

            if (p.PlayerId == OwnerId)
            {
                // never remove the owner of the room

                // don't remove player => update all player status to "Left" 
                // Close the game 
                // No more owner => cannot start the game

                foreach (var rp in _players)
                {
                    rp.Status = ERoomPlayerStatus.Left;
                }

                Status = ERoomStatus.Closed;
                
                
                return;
            }

            _players.Remove(p);
            if (_players.Count == 0)
            {
                //All rooms awaiting with no players are closed.
                // Owner has left the room. Should never happen
                Status = ERoomStatus.Closed;
            }
            return;
        }

        if (Status == ERoomStatus.Started) // if not required. stays for clarity
        {
            p.Status = ERoomPlayerStatus.Left; // Player has been disconnected / left mid-game. Should appear at the bottom of the leaderboard.
        }

        if (_players.TrueForAll(x => x.Status is ERoomPlayerStatus.Left))
        {
            //all players have left mid-game
            Status = ERoomStatus.Closed; // or Closed, per design
        }
    }


    public IReadOnlyList<LeaderboardEntryOutcome> ComputeLeaderboardEntries()
    {
        if (Status != ERoomStatus.Finished)
        {
            throw new DomainException("Leaderboard only available for finished games.");
        }

        var winnerId = WinnerPlayerId ?? throw new DomainException("Winner not set although room is finished.");

        return Players.Select(rp =>
        {
            var isWinner = rp.PlayerId == winnerId;
            var lower = rp.SecretIsGreaterThan + 1;
            var upper = rp.SecretIsLessThan - 1;
            var width = isWinner ? 0 : Math.Max(0, upper - lower);

            return new LeaderboardEntryOutcome(
                rp.PlayerId,
                rp.PlayerUsername,
                rp.Status == ERoomPlayerStatus.Finished,
                rp.Attempts,
                rp.FirstGuessAt,
                rp.LastGuessAt,
                lower,
                upper,
                width
            );
        }).ToList();
    }
}

public enum ERoomStatus
{
    AwaitingPlayers = 0,
    Started = 1,
    Finished = 2,
    Closed = 3,
}

public enum EGuessResult
{
    TooBig = 0,
    TooLow = 1,
    Win = 2,
}