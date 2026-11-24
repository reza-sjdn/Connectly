namespace Connectly.Application.Common.Interfaces;

public interface IIdentityService
{
    /// <summary>Creates the Identity account. Throws ConflictException / ValidationException on failure.</summary>
    Task<Guid> CreateUserAsync(string email, string password, CancellationToken ct = default);

    /// <summary>Returns the user id when credentials are valid, otherwise null (handles lockout).</summary>
    Task<Guid?> ValidateCredentialsAsync(string email, string password, CancellationToken ct = default);
}
