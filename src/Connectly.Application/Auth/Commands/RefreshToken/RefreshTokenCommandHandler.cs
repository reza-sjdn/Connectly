using Connectly.Application.Auth.Models;
using Connectly.Application.Common.Interfaces;
using MediatR;

namespace Connectly.Application.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler(ITokenService tokens)
    : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    public Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken ct)
        => tokens.RotateAsync(request.RefreshToken, ct);
}
