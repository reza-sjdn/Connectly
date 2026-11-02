using Connectly.Application.Auth.Models;
using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace Connectly.Application.Auth.Commands;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponse>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class LoginCommandHandler(IIdentityService identity, ITokenService tokens)
    : IRequestHandler<LoginCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken ct)
    {
        var userId = await identity.ValidateCredentialsAsync(request.Email, request.Password, ct)
                     ?? throw new AppUnauthorizedException("Invalid email or password.");
        return await tokens.IssueAsync(userId, ct);
    }
}
