using HiLoGame.Domain.Common;
namespace HiLoGame.Domain.Aggregates.Room.ValueObjects;

public sealed record GameRules
{
    public int Min { get; private set; }
    public int Max { get; private set; }
    public int MaxPlayers { get; private set; }

    private GameRules() { } // EF
    private GameRules(int min, int max, int maxPlayers)
    {
        if (min < 0) throw new DomainException("Min must be >= 0.");
        if (max <= min) throw new DomainException("Max must be > Min.");
        if (maxPlayers is < 1 or > 8) throw new DomainException("MaxPlayers must be 1..8.");

        Min = min; 
        Max = max; 
        MaxPlayers = maxPlayers; 
    }

    public static GameRules Create(int min, int max, int maxPlayers, int maxRounds) => new(min, max, maxPlayers);
    public static GameRules CreateDefault() => new(1, 15, 6);

    public void EnsureGuessInRange(int guess)
    {
        if (guess < Min || guess > Max) throw new DomainException("Guess out of range.");
    }

    public void EnsurePlayersAllowed(int newCount)
    {
        if (newCount < 1 || newCount > MaxPlayers)
        {
            throw new DomainException("Player limit exceeded.");
        }
    }

    public void EnsureValid()
    {
        if (Min < 0 || Max < 0) throw new DomainException("Must be positives numbers");
        if (Max == int.MaxValue) throw new DomainException("Max is too big");
        if (Min >= Max) throw new DomainException("Min must be < Max.");
        if (Max - Min > 1_000_000) throw new DomainException("Range too large for this rule.");
    }
}