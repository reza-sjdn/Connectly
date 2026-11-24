using Connectly.Application.Common.Interfaces;
using MediatR;

namespace Connectly.Application.Auth.Commands.Logout;

public class LogoutCommandHandler(ITokenService tokens) : IRequestHandler<LogoutCommand>
{
    public Task Handle(LogoutCommand request, CancellationToken ct)
        => tokens.RevokeAsync(request.RefreshToken, ct);
}
