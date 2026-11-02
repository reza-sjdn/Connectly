namespace Connectly.Application.Auth.Models;

public record AuthResponse(string AccessToken, string RefreshToken, DateTime AccessTokenExpiresAtUtc);
