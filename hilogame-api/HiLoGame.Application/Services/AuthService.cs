using HiLoGame.Application.Abstractions.Persistence;
using HiLoGame.Application.Common.Exceptions;
using HiLoGame.Application.Models.Auth;
using HiLoGame.Domain.Aggregates.Player;
using HiLoGame.Infrastructure.Auth;
using Microsoft.AspNetCore.Identity;

namespace HiLoGame.Application.Services
{
    public interface IAuthService
    {
        public Task<Player> RegisterAsync(string username, string password, CancellationToken ct = default);
        public Task<TokenResponse?> Login(string username, string password, CancellationToken ct = default);
        public Task<TokenResponse?> RefreshTokensAsync(RefreshTokenRequestResponse command, CancellationToken ct = default);
    }

    public class AuthService(
        IUnitOfWork unitOfWork,
        UserManager<Player> userManager,
        IJwtTokenService jwtTokenProviderService) : IAuthService
    {
        public async Task<TokenResponse?> Login(string username, string password, CancellationToken ct = default)
        {

            var player = await unitOfWork.PlayerRepository.GetByUsername(username, ct);
            if (player?.PasswordHash == null)
            {
                return null;
            }

            if (new PasswordHasher<Player>().VerifyHashedPassword(player, player.PasswordHash, password)
               == PasswordVerificationResult.Failed)
            {
                throw new Exception("Invalid password");
            }

            await GenerateAndSaveRefreshTokenAsync(player, ct);

            return new TokenResponse()
            {
                AccessToken = await jwtTokenProviderService.CreateToken(player),
                RefreshToken = player.RefreshToken!,
                ExpiresIn = player.RefreshTokenExpiryTime!.Value,
            };

        }

        public async Task<Player> RegisterAsync(string username, string password, CancellationToken ct)
        {

            if (await unitOfWork.PlayerRepository.AnyByUsername(username, ct))
            {
                throw new ForbiddenException("Username already exists");
            }

            var player = new Player(username);
            var result = await userManager.CreateAsync(player, password);

            if (!result.Succeeded)
            {
                throw new BadRequestException($"User registration failed");
            }
            return player;
        }

        private async Task<string> GenerateAndSaveRefreshTokenAsync(Player player, CancellationToken ct = default)
        {

            player.RefreshToken = jwtTokenProviderService.GenerateRefreshToken();
            player.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await unitOfWork.CommitAsync(ct);
            return player.RefreshToken;
        }

        private async Task<Player?> ValidateRefreshTokenAsync(string userId, string refreshToken, CancellationToken ct)
        {

            var user = await unitOfWork.PlayerRepository.GetById(userId, ct);
            if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return null;
            }
            return user;
        }

        public async Task<TokenResponse?> RefreshTokensAsync(RefreshTokenRequestResponse command, CancellationToken ct = default)
        {
            var user = await ValidateRefreshTokenAsync(command.UserId, command.RefreshToken, ct);
            if (user is null)
            {
                return null;
            }

            await GenerateAndSaveRefreshTokenAsync(user, ct);

            return new TokenResponse()
            {
                AccessToken = await jwtTokenProviderService.CreateToken(user),
                RefreshToken = user.RefreshToken!,
                ExpiresIn = user.RefreshTokenExpiryTime!.Value,
            };

        }
    }
}
