namespace HiLoGame.Application.Models.Auth;

public class RefreshTokenRequestResponse
{
    public required string UserId { get; set; }
    public required string RefreshToken { get; set; }
}