namespace HiLoGame.Application.Models.Rooms;

public sealed record RoomLeaderboardResponse(
    Guid RoomId,
    string WinnerPlayerId,
    string WinnerUsername,
    DateTime FinishedAt,
    IReadOnlyList<RoomLeaderboardEntry> Leaderboard
);

public sealed record RoomLeaderboardEntry(
    string PlayerId,
    string PlayerUsername,
    bool Finished,
    int Attempts,
    DateTime? FirstGuessAt,
    DateTime? LastGuessAt,
    int LowerBound,          // = SecretIsGreaterThan
    int UpperBound,          // = SecretIsLessThan
    int CurrentSearchSpace  // = UpperBound - LowerBound - 1
);