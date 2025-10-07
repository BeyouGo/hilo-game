namespace HiLoGame.Domain.Aggregates.Room.Outcomes;

public sealed record LeaderboardEntryOutcome(
    string PlayerId,
    string PlayerUsername,
    bool Finished,
    int Attempts,
    DateTime? FirstGuessAt,
    DateTime? LastGuessAt,
    int LowerBound,
    int UpperBound,
    int Width);