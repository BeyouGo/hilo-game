using HiLoGame.Domain.Common;

namespace HiLoGame.Domain.Aggregates.Room.Entities;

public sealed class RoomPlayer
{
    public Room Room { get; set; } = default!;
    public Guid RoomId { get; }
    public string PlayerId { get; }
    public string PlayerUsername { get; }
    public int SecretIsGreaterThan { get; set; } 
    public int SecretIsLessThan { get; set; } // boundaries the player has manager to guess

    public DateTime? FirstGuessAt { get; set; }
    public DateTime? LastGuessAt { get; set; }

    public ERoomPlayerStatus Status { get; set; }

    public int Attempts { get; set; } // including the attempt needed for the win

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    private RoomPlayer() { } // EF
    internal RoomPlayer(Guid roomId, string playerId, string playerUsername)
    {
        RoomId = roomId;
        PlayerId = playerId ?? throw new DomainException("PlayerId can't be empty");
        PlayerUsername = playerUsername;
        Status = ERoomPlayerStatus.Waiting;
    }
}

public enum ERoomPlayerStatus
{
    Waiting = 0,
    Playing = 1,
    Finished = 2,
    Left = 4,
}