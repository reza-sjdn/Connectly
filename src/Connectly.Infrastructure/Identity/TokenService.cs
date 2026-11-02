using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Connectly.Application.Auth.Models;
using Connectly.Application.Common.Exceptions;
using Connectly.Application.Common.Interfaces;
using Connectly.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Connectly.Infrastructure.Identity;

public class TokenService(
    AppDbContext db,
    UserManager<ApplicationUser> users,
    IOptions<JwtSettings> options,
    TimeProvider clock) : ITokenService
{
    private readonly JwtSettings _jwt = options.Value;

    public async Task<AuthResponse> IssueAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await users.FindByIdAsync(userId.ToString())
                   ?? throw new NotFoundException(nameof(ApplicationUser), userId);

        var (response, _) = Build(user);
        await db.SaveChangesAsync(ct);
        return response;
    }

    public async Task<AuthResponse> RotateAsync(string refreshToken, CancellationToken ct = default)
    {
        var now = clock.GetUtcNow().UtcDateTime;
        var hash = Hash(refreshToken);

        var stored = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, ct)
                     ?? throw new AppUnauthorizedException("Invalid refresh token.");

        if (stored.RevokedAtUtc is not null)
        {
            // A rotated token was presented again: assume theft and kill every session of this user.
            await db.RefreshTokens
                .Where(t => t.UserId == stored.UserId && t.RevokedAtUtc == null)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAtUtc, now), ct);
            throw new AppUnauthorizedException("Refresh token was already used.");
        }

        if (stored.ExpiresAtUtc <= now) throw new AppUnauthorizedException("Refresh token expired.");

        var user = await users.FindByIdAsync(stored.UserId.ToString())
                   ?? throw new AppUnauthorizedException("Invalid refresh token.");

        var (response, newHash) = Build(user);
        stored.RevokedAtUtc = now;
        stored.ReplacedByHash = newHash;
        await db.SaveChangesAsync(ct);
        return response;
    }

    public async Task RevokeAsync(string refreshToken, CancellationToken ct = default)
    {
        var hash = Hash(refreshToken);
        var stored = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, ct);
        if (stored is { RevokedAtUtc: null })
        {
            stored.RevokedAtUtc = clock.GetUtcNow().UtcDateTime;
            await db.SaveChangesAsync(ct);
        }
    }

    // Builds the token pair and tracks the new refresh token (caller saves).
    private (AuthResponse Response, string RefreshHash) Build(ApplicationUser user)
    {
        var now = clock.GetUtcNow().UtcDateTime;
        var expires = now.AddMinutes(_jwt.AccessTokenMinutes);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            ]),
            Issuer = _jwt.Issuer,
            Audience = _jwt.Audience,
            NotBefore = now,
            Expires = expires,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret)),
                SecurityAlgorithms.HmacSha256)
        };
        var accessToken = new JsonWebTokenHandler().CreateToken(descriptor);

        var rawRefresh = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var refreshHash = Hash(rawRefresh);
        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshHash,
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddDays(_jwt.RefreshTokenDays)
        });

        return (new AuthResponse(accessToken, rawRefresh, expires), refreshHash);
    }

    private static string Hash(string token)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
