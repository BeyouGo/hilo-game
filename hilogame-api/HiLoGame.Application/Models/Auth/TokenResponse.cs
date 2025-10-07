namespace HiLoGame.Application.Models.Auth;

public class TokenResponse
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
    public required DateTime ExpiresIn { get; set; }
}