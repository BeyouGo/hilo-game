using HiLoGame.Application.Services;
using MediatR;

namespace HiLoGame.Application.Features.Authentication;

public class RegisterPlayer
{
    public record Command(string UserName, string Password) : IRequest<bool>;

    public class Handler(IAuthService authService) : IRequestHandler<Command, bool>
    {
        public async Task<bool> Handle(Command message, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(message.UserName))
            {
                throw new Exception("Invalid username exception");
            }

            var player = await authService.RegisterAsync(message.UserName, message.Password, ct);
            if (player == null)
            {
                throw new Exception("User can't be created");
            }

            return true;
        }
    }
}