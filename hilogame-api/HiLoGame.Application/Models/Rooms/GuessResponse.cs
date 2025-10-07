using HiLoGame.Domain.Aggregates.Room.Entities;

namespace HiLoGame.Application.Models.Rooms;

public class GuessResponse
{
    public required string PlayerId { get; set; }
    public Guid RoomId { get; set; }
    public int Guess { get; set; }
    public EGuessResult Result { get; set; }
    public int AttemptsAfterThisGuess { get; set; }
    public DateTime GuessedAt { get; set; }
    public int SecretIsBiggerThan { get; set; }
    public int SecretIsLessThan { get; set; }
    public bool Finished { get; set; }
}
