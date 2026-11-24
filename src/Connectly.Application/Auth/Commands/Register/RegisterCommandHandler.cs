using Connectly.Application.Auth.Models;
using Connectly.Application.Common.Interfaces;
using Connectly.Domain.Entities.UserProfiles;
using MediatR;

namespace Connectly.Application.Auth.Commands.Register;

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
