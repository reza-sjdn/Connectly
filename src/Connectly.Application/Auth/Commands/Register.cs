using Connectly.Application.Auth.Models;
using Connectly.Application.Common.Interfaces;
using Connectly.Domain.Entities;
using FluentValidation;
using MediatR;

namespace Connectly.Application.Auth.Commands;

public record RegisterCommand(string Email, string Password, string FirstName, string LastName)
    : IRequest<AuthResponse>;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
    }
}

public class RegisterCommandHandler(
    IIdentityService identity,
    ITokenService tokens,
    IApplicationDbContext db) : IRequestHandler<RegisterCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken ct)
    {
        var userId = await identity.CreateUserAsync(request.Email, request.Password, ct);

        db.UserProfiles.Add(new UserProfile
        {
            Id = userId,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim()
        });
        await db.SaveChangesAsync(ct);

        return await tokens.IssueAsync(userId, ct);
    }
}
