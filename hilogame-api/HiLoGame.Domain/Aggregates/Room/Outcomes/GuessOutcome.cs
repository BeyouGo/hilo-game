using HiLoGame.Domain.Aggregates.Room.Entities;

namespace HiLoGame.Domain.Aggregates.Room.Outcomes;

public sealed record GuessOutcome(
    EGuessResult Result,
    DateTime GuessedAt,
    int Guess,
    int Attempts,
    bool IsWinningGuess,
    int SecretIsGreaterThan,
    int SecretIsLessThan
);