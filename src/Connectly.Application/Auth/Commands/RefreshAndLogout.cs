using Connectly.Application.Auth.Models;
using Connectly.Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace Connectly.Application.Auth.Commands;

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResponse>;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator() => RuleFor(x => x.RefreshToken).NotEmpty();
}

public class RefreshTokenCommandHandler(ITokenService tokens)
    : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    public Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken ct)
        => tokens.RotateAsync(request.RefreshToken, ct);
}

public record LogoutCommand(string RefreshToken) : IRequest;

public class LogoutCommandHandler(ITokenService tokens) : IRequestHandler<LogoutCommand>
{
    public Task Handle(LogoutCommand request, CancellationToken ct)
        => tokens.RevokeAsync(request.RefreshToken, ct);
}
