using HiLoGame.Application.Common.Exceptions;
using HiLoGame.Application.Models.Auth;
using HiLoGame.Application.Services;
using MediatR;

namespace HiLoGame.Application.Features.Authentication;

public class LoginPlayer
{
    public record Command(string Username, string Password) : IRequest<TokenResponse?>;
    public class Handler(IAuthService authService) : IRequestHandler<Command, TokenResponse?>
    {
        public async Task<TokenResponse?> Handle(Command message, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(message.Username) || string.IsNullOrWhiteSpace(message.Password))
            {
                throw new BadRequestException("No user info provided");
            }

            var token = await authService.Login(message.Username, message.Password, ct);
            
            return token ?? throw new BadRequestException("Wrong credentials");
        }
    }
}