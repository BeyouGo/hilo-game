using HiLoGame.Domain.Aggregates.Player;

namespace HiLoGame.Infrastructure.Auth;

public interface IJwtTokenService
{
    Task<string> CreateToken(Player user);
    string GenerateRefreshToken();
}