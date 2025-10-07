using HiLoGame.Application.Abstractions.Persistence;
using HiLoGame.Application.Abstractions.Realtime;
using HiLoGame.Application.Services;
using HiLoGame.Domain.Services;
using HiLoGame.Infrastructure;
using HiLoGame.Infrastructure.Auth;
using HiLoGame.Infrastructure.Realtime;
using HiLoGame.Infrastructure.Repositories;

namespace HiLoGame.Api;

public static class ServicesConfigurations
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<ISecretGenerator, CryptoSecretGenerator>();
        services.AddScoped<IGameNotifier, GameNotifier>();

        ConfigureRepositories(services);
    }

    private static void ConfigureRepositories(IServiceCollection services)
    {
        services.AddScoped<IPlayerRepository, PlayerRepository>();
        services.AddScoped<IRoomRepository, RoomRepository>();
    }

}