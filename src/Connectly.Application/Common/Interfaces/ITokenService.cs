using Connectly.Application.Auth.Models;

namespace Connectly.Application.Common.Interfaces;

public interface ITokenService
{
    Task<AuthResponse> IssueAsync(Guid userId, CancellationToken ct = default);
    Task<AuthResponse> RotateAsync(string refreshToken, CancellationToken ct = default);
    Task RevokeAsync(string refreshToken, CancellationToken ct = default);
}
