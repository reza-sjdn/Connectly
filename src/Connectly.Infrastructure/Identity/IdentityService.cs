using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;

namespace Connectly.Infrastructure.Identity;

public class IdentityService(UserManager<ApplicationUser> users) : IIdentityService
{
    public async Task<Guid> CreateUserAsync(string email, string password, CancellationToken ct = default)
    {
        var user = new ApplicationUser { UserName = email, Email = email };
        var result = await users.CreateAsync(user, password);
        if (result.Succeeded) return user.Id;

        if (result.Errors.Any(e => e.Code is "DuplicateEmail" or "DuplicateUserName"))
            throw new ConflictException("An account with this email already exists.");

        throw new ValidationException(result.Errors.Select(e => new ValidationFailure(e.Code, e.Description)));
    }

    public async Task<Guid?> ValidateCredentialsAsync(string email, string password, CancellationToken ct = default)
    {
        var user = await users.FindByEmailAsync(email);
        if (user is null) return null;
        if (await users.IsLockedOutAsync(user)) return null;

        if (!await users.CheckPasswordAsync(user, password))
        {
            await users.AccessFailedAsync(user);
            return null;
        }

        await users.ResetAccessFailedCountAsync(user);
        return user.Id;
    }
}
